namespace MyFirstExcelAddin
{
    partial class MyRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public MyRibbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tab1 = this.Factory.CreateRibbonTab();
            this.group1 = this.Factory.CreateRibbonGroup();
            this.btnConvertToAlphanumeric = this.Factory.CreateRibbonButton();
            this.btnRevertToOriginal = this.Factory.CreateRibbonButton();
            this.tab1.SuspendLayout();
            this.group1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tab1
            // 
            this.tab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tab1.Groups.Add(this.group1);
            this.tab1.Label = "TabAddIns";
            this.tab1.Name = "tab1";
            // 
            // group1
            // 
            this.group1.Items.Add(this.btnConvertToAlphanumeric);
            this.group1.Items.Add(this.btnRevertToOriginal);
            this.group1.Label = "Sanitize Text";
            this.group1.Name = "group1";
            // 
            // btnConvertToAlphanumeric
            // 
            this.btnConvertToAlphanumeric.Label = "Convert to Alphanumeric";
            this.btnConvertToAlphanumeric.Name = "btnConvertToAlphanumeric";
            this.btnConvertToAlphanumeric.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnConvertToAlphanumeric_Click);
            // 
            // btnRevertToOriginal
            // 
            this.btnRevertToOriginal.Label = "Revert to Original";
            this.btnRevertToOriginal.Name = "btnRevertToOriginal";
            this.btnRevertToOriginal.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnRevertToOriginal_Click);
            // 
            // MyRibbon
            // 
            this.Name = "MyRibbon";
            this.RibbonType = "Microsoft.Excel.Workbook";
            this.Tabs.Add(this.tab1);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.MyRibbon_Load);
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.group1.ResumeLayout(false);
            this.group1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnConvertToAlphanumeric;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnRevertToOriginal;
    }

    partial class ThisRibbonCollection
    {
        internal MyRibbon MyRibbon
        {
            get { return this.GetRibbon<MyRibbon>(); }
        }
    }
}
