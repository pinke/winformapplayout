﻿using Controls.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ControlsTest
{
    public partial class TSDataGridViewForm : Form
    {
        private Model.Data.DataTable _dataTable = null;
        //private TSGridViewData _data = null;
        public TSDataGridViewForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, System.EventArgs e)
        {
            List<TSGridColumn> columns;
            _dataTable = LoadData(out columns);
             
            //_data = new TSGridViewData(_dataTable, columns);

            ////this.dataGridView1.DataSource = _dataTable.Rows;
            //this.dataGridView1.DataSource = _data;
        }

        private Model.Data.DataTable LoadData(out List<TSGridColumn> columns)
        {
            Model.Data.DataTable dataTable = new Model.Data.DataTable 
            {
                Rows = new List<Model.Data.DataRow>(),
                ColumnIndex = new Dictionary<string,int>()
            };

            columns = new List<TSGridColumn>();
            Random ran = new Random();

            for (int i = 0; i < 15; i++)
            {
                Model.Data.DataRow dataRow = new Model.Data.DataRow 
                {
                    Columns = new List<Model.Data.DataValue>()
                };

                for (int j = 0; j < 10; j++)
                {
                    Model.Data.DataValue dataValue = new Model.Data.DataValue();
                    dataValue.Type = Model.Data.DataValueType.Int;
                    dataValue.Value = ran.Next(150);

                    if (i == 0)
                    {
                        string name = string.Format("T-{0}", j);
                        if (!dataTable.ColumnIndex.ContainsKey(name))
                        {
                            dataTable.ColumnIndex.Add(name, j);
                        }

                        TSGridColumn column = new TSGridColumn();
                        column.Name = name;
                        column.Text = name;
                        columns.Add(column);
                    }

                    dataRow.Columns.Add(dataValue);
                }

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Model.Data.DataRow dataRow = new Model.Data.DataRow
            {
                Columns = new List<Model.Data.DataValue>()
            };

            Random ran = new Random();
            for (int j = 0; j < 10; j++)
            {
                Model.Data.DataValue dataValue = new Model.Data.DataValue();
                dataValue.Type = Model.Data.DataValueType.Int;
                dataValue.Value = ran.Next(150);
                dataRow.Columns.Add(dataValue);
            }
            _dataTable.Rows.Add(dataRow);

        }
    }
}
