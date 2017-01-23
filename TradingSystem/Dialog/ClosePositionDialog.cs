﻿using Config;
using Controls.Entity;
using Controls.GridView;
using Model.Binding.BindingUtil;
using Model.UI;
using System;
using System.Collections.Generic;
using Util;

namespace TradingSystem.Dialog
{
    public partial class ClosePositionDialog : Forms.BaseDialog
    {
        private const string GridId = "closepositioninstance";

        private const string msgInvalidDate = "opendialogvaliddate";
        private const string msgInvalidTime = "opendialogvalidtime";

        private GridConfig _gridConfig;

        private SortableBindingList<ClosePositionInstanceItem> _dataSource = new SortableBindingList<ClosePositionInstanceItem>(new List<ClosePositionInstanceItem>());

        public ClosePositionDialog()
            :base()
        {
            InitializeComponent();
        }

        public ClosePositionDialog(GridConfig gridConfig)
            : this()
        {
            _gridConfig = gridConfig;

            LoadControl += new FormLoadHandler(Form_LoadControl);
            LoadData += new FormLoadHandler(Form_LoadData);

            this.btnConfirm.Click += new EventHandler(Button_Confirm);
            this.btnCancel.Click += new EventHandler(Button_Cancel);
        }

        #region load control

        private bool Form_LoadControl(object sender, object data)
        {
            //Load Command Trading
            TSDataGridViewHelper.AddColumns(this.instGridView, _gridConfig.GetGid(GridId));
            Dictionary<string, string> gridColDataMap = GridViewBindingHelper.GetPropertyBinding(typeof(ClosePositionInstanceItem));
            TSDataGridViewHelper.SetDataBinding(this.instGridView, gridColDataMap);
            this.instGridView.DataSource = _dataSource;

            return true;
        }

        #endregion

        #region load data

        private bool Form_LoadData(object sender, object data)
        {
            if (data == null)
            {
                return false;
            }

            if (!(data is List<ClosePositionInstanceItem>))
            {
                return false;
            }

            var ds = data as List<ClosePositionInstanceItem>;
            ds.ForEach(p => _dataSource.Add(p));

            return true;
        }

        #endregion

        #region button click event handler

        private void Button_Confirm(object sender, EventArgs e)
        {
            if (!ValidateDate())
            {
                MessageDialog.Error(this, msgInvalidDate);

                return;
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void Button_Cancel(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        #endregion

        #region GetData

        public override object GetData()
        {
            var dataList = new List<ClosePositionInstanceItem>();
            foreach (var item in _dataSource)
            {
                dataList.Add(item);
            }

            return dataList;
        }

        #endregion

        private bool ValidateDate()
        {
            foreach (var item in _dataSource)
            {
                if (!DateUtil.IsValidDate(item.StartDate))
                {
                    return false;
                }

                if (!DateUtil.IsValidDate(item.EndDate))
                {
                    return false;
                }

                if (!DateUtil.IsValidTime(item.StartTime))
                {
                    return false;
                }

                if (!DateUtil.IsValidTime(item.EndTime))
                {
                    return false;
                }

                if (item.StartDate > item.EndDate)
                {
                    return false;
                }

                if (item.StartDate == item.EndDate && item.StartTime >= item.EndTime)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
