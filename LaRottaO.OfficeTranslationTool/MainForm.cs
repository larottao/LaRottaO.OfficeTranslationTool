using LaRottaO.OfficeTranslationTool.Models;
using LaRottaO.OfficeTranslationTool.Utils;

using System.ComponentModel;
using static LaRottaO.OfficeTranslationTool.GlobalVariables;

namespace LaRottaO.OfficeTranslationTool
{
    public partial class MainForm : Form
    {
        private FormLogic formLogic;

        public MainForm()
        {
            InitializeComponent();
            formLogic = new FormLogic(dataGridView);
            dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView.MultiSelect = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var loadSettingsResult = LoadProgramSettings.load();

            if (!loadSettingsResult.success)
            {
                UIHelpers.showErrorMessage(loadSettingsResult.errorReason);
            }

            foreach (KeyValuePair<String, String> lang in AVAILABLE_LANGUAGES)
            {
                comboBoxSourceLanguage.Items.Add(lang.Key);
                comboBoxDestLanguage.Items.Add(lang.Key);
            }
        }

        private void buttonOpenOfficeFile_Click(object sender, EventArgs e)
        {
            formLogic.openOfficeFile();
        }

        //**************************************************
        //Changes the color of the DataGridView to stripes
        //**************************************************

        private void dataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                if (i % 2 == 0)
                {
                    dataGridView.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            formLogic.closeOfficeFile();
            //TODO ALSO CLOSE THE APP
        }

        private void button1_Click(object sender, EventArgs e)
        {
            formLogic.test();
        }

        private void dataGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                var property = typeof(ShapeElement).GetProperty(column.DataPropertyName);
                if (property != null)
                {
                    // Check for [Browsable(false)]
                    if (property.GetCustomAttributes(typeof(BrowsableAttribute), true).FirstOrDefault() is BrowsableAttribute browsable && !browsable.Browsable)
                    {
                        column.Visible = false; // Hide the column
                    }

                    // Check for [ColumnName] and set the header text
                    if (property.GetCustomAttributes(typeof(ColumnNameAttribute), true).FirstOrDefault() is ColumnNameAttribute columnName)
                    {
                        column.HeaderText = columnName.Name;
                    }
                }
            }
        }

        private void dataGridView_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
        {
        }

        private String? previousCellValue;

        private void dataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (!formLogic.areBothSourceAndDestintionLanguagesSet())
            {
                UIHelpers.showInformationMessage("Please select the Source and Target languages first.");
                //TODO this doesnt work
                dataGridView.ClearSelection();
                return;
            }

            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var cell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];

                previousCellValue = cell.Value?.ToString();
            }
        }

        private void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var cell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];

                string? newValue = cell.Value?.ToString();

                if (newValue != null && !newValue.Equals(previousCellValue))
                {
                    formLogic.saveNewTranslationTypedByUser(e.RowIndex, e.ColumnIndex, newValue);
                }
            }
        }

        private void comboBoxSourceLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            formLogic.setDictionaryLanguage(comboBoxSourceLanguage.Text, comboBoxDestLanguage.Text);
        }

        private void comboBoxDestLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            formLogic.setDictionaryLanguage(comboBoxSourceLanguage.Text, comboBoxDestLanguage.Text);
        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void buttonTranslateAll_Click(object sender, EventArgs e)
        {
            if (!formLogic.areBothSourceAndDestintionLanguagesSet())
            {
                UIHelpers.showInformationMessage("Please select the Source and Target languages first.");
                return;
            }

            var transResult = formLogic.translateAllShapeElements(comboBoxSourceLanguage.Text, comboBoxDestLanguage.Text);

            if (transResult.success)
            {
                UIHelpers.showInformationMessage("Process complete");
            }
            else
            {
                UIHelpers.showErrorMessage(transResult.errorReason);
            }
        }

        private void dataGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView.Rows.Count == 0)
            {
                return;
            }

            formLogic.userClickedRow(e.RowIndex, e.ColumnIndex);
        }
    }
}