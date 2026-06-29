using System;
using Grasshopper.Kernel;

namespace AdvancedArchitectureTools
{
    /// <summary>
    /// A high-performance, bidirectional pipeline counter that safely handles nested cluster expiration.
    /// </summary>
    public class BidirectionalClusterCounter : GH_Component
    {
        // Global state variables across solver iterations
        private double _currentValue = 0.0;
        private double _actualStep = 1.0;
        private int _iterationCount = 0;
        private bool _isRunning = false;

        /// <summary>
        /// Initializes a new instance of the BidirectionalClusterCounter class.
        /// </summary>
        public BidirectionalClusterCounter()
          : base("Counter", "Count",
              "A sequence counter.",
              "Alligator", "Utils")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "R", "Start/Stop boolean trigger.", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Start", "S", "The starting value of the sequence.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Step", "St", "The absolute step amount per iteration.", GH_ParamAccess.item, 1.0);

            // End has no default. This allows us to detect if the user explicitly provided a value.
            pManager.AddNumberParameter("End", "E", "Target value. Overrides Count if both are provided.", GH_ParamAccess.item);
            pManager[3].Optional = true;

            pManager.AddIntegerParameter("Count", "C", "Number of steps to take if End is not provided.", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("IntervalMs", "I", "Time interval strictly in milliseconds.", GH_ParamAccess.item, 500);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Value", "V", "Current sequence value.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Core evaluation loop for data processing and schedule triggering.
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Clear any previous warnings from the component node
            this.ClearRuntimeMessages();

            // 1. Initialize local variables
            bool run = false;
            double start = 0.0;
            double step = 1.0;
            double endTarget = 0.0;
            int countTarget = 10;
            int interval = 500;

            // 2. Safely retrieve required inputs
            if (!DA.GetData(0, ref run)) return;
            if (!DA.GetData(1, ref start)) return;
            if (!DA.GetData(2, ref step)) return;
            DA.GetData(5, ref interval);

            // 3. Determine which termination logic takes precedence
            bool hasEnd = DA.GetData(3, ref endTarget);
            bool hasCount = DA.GetData(4, ref countTarget);

            if (hasEnd && this.Params.Input[4].SourceCount > 0)
            {
                // Warn the user visually on the canvas that their Count input is being ignored
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Both 'End' and 'Count' are connected. 'End' value takes priority.");
            }

            // 4. Handle explicit stop or reset condition
            if (!run)
            {
                _currentValue = start;
                _iterationCount = 0;
                _isRunning = false;
                DA.SetData(0, null);
                return;
            }

            // 5. Handle the rising edge (Initial Start Configuration)
            if (run && !_isRunning)
            {
                _currentValue = start;
                _iterationCount = 0;

                if (hasEnd)
                {
                    // Validate against impossible geometric trajectories
                    if (start < endTarget && step < 0)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Start < End but Step is negative. This causes an infinite loop.");
                        return;
                    }

                    // Automatically vector the step direction based on start and end relationships
                    _actualStep = (start <= endTarget) ? Math.Abs(step) : -Math.Abs(step);
                }
                else
                {
                    // If relying strictly on Count, accept the step direction exactly as the user provided it
                    _actualStep = step;
                }

                _isRunning = true;
            }

            // 6. Evaluate termination conditions before outputting
            if (_isRunning)
            {
                if (hasEnd)
                {
                    // Halt if the current value has passed the target in either direction
                    if ((_actualStep > 0 && _currentValue >= endTarget) ||
                        (_actualStep < 0 && _currentValue <= endTarget))
                    {
                        _currentValue = endTarget; // Clamp to exact target mathematically
                        _isRunning = false;
                    }
                }
                else
                {
                    // Halt if the maximum iteration limit is reached
                    if (_iterationCount >= countTarget)
                    {
                        _isRunning = false;
                    }
                }
            }

            // 7. Output the current valid sequence value
            DA.SetData(0, _currentValue);

            // 8. Schedule the next pipeline execution if still active
            int safeInterval = Math.Max(5, interval);

            if (_isRunning)
            {
                GH_Document topDoc = GetTopLevelDocument(this.OnPingDocument());
                if (topDoc != null)
                {
                    topDoc.ScheduleSolution(safeInterval, ScheduleCallback);
                }
            }
        }

        /// <summary>
        /// Callback executed natively by the Grasshopper document schedule.
        /// </summary>
        /// <param name="doc">The active Grasshopper document.</param>
        private void ScheduleCallback(GH_Document doc)
        {
            if (_isRunning)
            {
                // 1. Advance the mathematical state
                _currentValue += _actualStep;
                _iterationCount++;

                // 2. Expire the local component safely
                this.ExpireSolution(false);

                // 3. Bubble up the expiration through all parent clusters
                GH_Document currentDoc = this.OnPingDocument();

                while (currentDoc != null && currentDoc.Owner != null)
                {
                    IGH_ActiveObject activeOwner = currentDoc.Owner as IGH_ActiveObject;
                    if (activeOwner != null)
                    {
                        activeOwner.ExpireSolution(false);
                    }

                    IGH_DocumentObject docOwner = currentDoc.Owner as IGH_DocumentObject;
                    if (docOwner != null)
                    {
                        currentDoc = docOwner.OnPingDocument();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Traverses the document owner hierarchy to locate the root Grasshopper document.
        /// </summary>
        /// <param name="startingDoc">The immediate document containing this component.</param>
        /// <returns>The top-level Grasshopper document.</returns>
        private GH_Document GetTopLevelDocument(GH_Document startingDoc)
        {
            GH_Document current = startingDoc;

            while (current != null && current.Owner != null)
            {
                IGH_DocumentObject docOwner = current.Owner as IGH_DocumentObject;
                if (docOwner != null)
                {
                    current = docOwner.OnPingDocument();
                }
                else
                {
                    break;
                }
            }

            return current;
        }

        protected override System.Drawing.Bitmap Icon
        {
            get { return null; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("2B3C4D5E-6F7A-8B9C-0D1E-2F3A4B5C6D7E"); }
        }
    }
}
