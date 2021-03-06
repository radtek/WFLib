﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WFNetLib.ADO.DBToolGenerate;
using System.Collections;

namespace SQLDBTool
{
    public partial class frmGenerator : Form
    {
        DataAccess da = new DataAccess();        
        public frmGenerator()
        {
            InitializeComponent();
        }

        private void frmGenerator_Load(object sender, EventArgs e)
        {
            txtAssembly.Text = Properties.Settings.Default.strAA;
            DataTable dsTable = da.GetTables();
            if (DataAccess.Type == "access")
            {
                ArrayList drs = new ArrayList();
                foreach (DataRow dr in dsTable.Rows)
                {
                    if (dr["TABLE_NAME"].ToString().IndexOf("aspnet_") != -1)
                        drs.Add(dr);
                }
                for (int i = 0; i < drs.Count; i++)
                    dsTable.Rows.Remove((DataRow)drs[i]);
                cBoxTable.DataSource = dsTable.DefaultView;
                cBoxTable.DisplayMember = "TABLE_NAME";
            }
            else if(DataAccess.Type=="sql")
            {
                cBoxTable.DataSource = dsTable.DefaultView;
                cBoxTable.DisplayMember = "TableName";
            }
               
        }        
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.strAA = txtAssembly.Text;
            Properties.Settings.Default.Save();
            Generate gg = new Generate();
            gg.bWeb = bWeb.Checked;
            gg.bDLL = bDLL.Checked;
            gg.table_name = cBoxTable.Text;
            gg.dtColomns = da.GetColumnsByTable(gg.table_name);
            gg.dtPrimaryKey = da.GetPrimaryKeyByTable(gg.table_name);
            dataGridView1.DataSource = gg.dtPrimaryKey;
            dataGridView2.DataSource = gg.dtColomns;
            if(DataAccess.Type=="sql")
            {
                gg.ColumnName = "ColumnName";
                gg.ColumnType = "ColumnType";
                gg.type = "sql";
                gg.ColumnDefault = "sfwerw";
            }
            else if (DataAccess.Type == "access")
            {
                gg.ColumnName = "COLUMN_NAME";
                gg.ColumnType = "DATA_TYPE";
                gg.type = "access";
                gg.ColumnDefault = "COLUMN_DEFAULT";
            }
            if (gg.dtPrimaryKey.Rows.Count > 0)
            {
                DataRow dr = gg.dtPrimaryKey.Rows[0];
                gg.PrimaryKey = dr["COLUMN_NAME"].ToString();
            }            
            txtResult.Text=gg.Getpropertity(txtAssembly.Text);
            //GetDataReaderFactory();
        } 
        private void txtResult_KeyDown(object sender, KeyEventArgs e)
        {
            TextBoxSelectAll(txtResult, e);
        }

        private void TextBoxSelectAll(TextBox t, KeyEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control)==Keys.Control )
                if(e.KeyCode==Keys.A)
                    t.SelectAll();
        }

        private void txtReaderFactory_KeyDown(object sender, KeyEventArgs e)
        {
            TextBoxSelectAll(txtReaderFactory, e);
        }
    }
}