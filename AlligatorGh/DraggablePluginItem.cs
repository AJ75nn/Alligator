using System;
using System.Drawing;
using System.Windows.Forms;

namespace AlligatorGh
{
    public class DraggablePluginItem : UserControl
    {
        private Label _lblHandle;
        private CheckBox _chkVisible;
        private PictureBox _picIcon;
        private Label _lblName;
        private Label _lblCategoryCount;
        private Label _lblComponentCount;
        private bool _isSelected;
        private bool _showIcon = false;

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

        public Image PluginIcon
        {
            get => _picIcon.Image;
            set => _picIcon.Image = value;
        }

        public bool ShowIcon
        {
            get => _showIcon;
            set
            {
                if (_showIcon != value)
                {
                    _showIcon = value;
                    _picIcon.Visible = value;
                }
            }
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

        public int CategoryCount { set { _lblCategoryCount.Text = value.ToString(); } }
        public int ComponentCount { set { _lblComponentCount.Text = value.ToString(); } }

        public DraggablePluginItem()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(500, 40);
            this.BackColor = Color.White;
            this.Margin = new Padding(0);

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100f));

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

            _picIcon = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 24,
                Height = 24,
                Margin = new Padding(3),
                Visible = false // Default off until globally toggled
            };

            _lblName = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                Font = new Font("Arial", 10, FontStyle.Regular),
                Padding = new Padding(5, 0, 0, 0)
            };

            _lblCategoryCount = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 9, FontStyle.Bold),
            };

            _lblComponentCount = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 9, FontStyle.Bold),
            };

            // Wire up events
            _lblHandle.MouseDown += LblHandle_MouseDown;
            _chkVisible.CheckedChanged += ChkVisible_CheckedChanged;

            // Allow selection by clicking the background or the text label
            this.Click += OnItemClick;
            table.Click += OnItemClick;
            _lblName.Click += OnItemClick;
            _picIcon.Click += OnItemClick;
            _lblCategoryCount.Click += OnItemClick;
            _lblComponentCount.Click += OnItemClick;

            table.Controls.Add(_lblHandle, 0, 0);
            table.Controls.Add(_chkVisible, 1, 0);
            table.Controls.Add(_picIcon, 2, 0);
            table.Controls.Add(_lblName, 3, 0);
            table.Controls.Add(_lblCategoryCount, 4, 0);
            table.Controls.Add(_lblComponentCount, 5, 0);

            this.Controls.Add(table);
        }

        private void LblHandle_MouseDown(object sender, MouseEventArgs e)
        {
            // Important for allowing parent to intercept dragging smoothly
            _lblHandle.Capture = false;
            HandleMouseDown?.Invoke(this, e);
        }

        private void ChkVisible_CheckedChanged(object sender, EventArgs e)
        {
            VisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnItemClick(object sender, EventArgs e)
        {
            // The parent form will handle multi-selection logic using modifier keys
            this.Focus();
        }
    }
}
