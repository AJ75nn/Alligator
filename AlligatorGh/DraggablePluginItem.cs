using System;
using System.Drawing;
using System.Windows.Forms;

namespace AlligatorGh
{
    public class DraggablePluginItem : UserControl
    {
        private Label _lblHandle;
        private CheckBox _chkVisible;
        private Label _lblName;
        private bool _isSelected;

        public event EventHandler SelectionChanged;
        public event MouseEventHandler HandleMouseDown;
        public event EventHandler VisibilityChanged;

        public string PluginName
        {
            get => _lblName.Text;
            set => _lblName.Text = value;
        }

        public bool IsVisible
        {
            get => _chkVisible.Checked;
            set => _chkVisible.Checked = value;
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    this.BackColor = _isSelected ? Color.LightBlue : Color.White;
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public CheckBox CheckBox => _chkVisible;

        public DraggablePluginItem()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(300, 30);
            this.BackColor = Color.White;
            this.Margin = new Padding(0);

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0),
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            _lblHandle = new Label
            {
                Text = "≡",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.SizeAll
            };

            _chkVisible = new CheckBox
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                Text = "", // CheckBox has no text, so clicking it strictly toggles
                Padding = new Padding(5, 0, 0, 0)
            };

            _lblName = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            // Wire up events
            _lblHandle.MouseDown += (s, e) =>
            {
                // Important for allowing parent to intercept dragging smoothly
                _lblHandle.Capture = false;
                HandleMouseDown?.Invoke(this, e);
            };

            _chkVisible.CheckedChanged += (s, e) =>
            {
                VisibilityChanged?.Invoke(this, EventArgs.Empty);
            };

            // Allow selection by clicking the background or the text label
            this.Click += OnItemClick;
            table.Click += OnItemClick;
            _lblName.Click += OnItemClick;

            table.Controls.Add(_lblHandle, 0, 0);
            table.Controls.Add(_chkVisible, 1, 0);
            table.Controls.Add(_lblName, 2, 0);

            this.Controls.Add(table);
        }

        private void OnItemClick(object sender, EventArgs e)
        {
            // The parent form will handle multi-selection logic using modifier keys
            this.Focus();
        }
    }
}
