﻿using BLL.Manager;
using Config;
using Controls.Entity;
using Controls.GridView;
using Model.Binding.BindingUtil;
using Model.UI;
using System.Collections.Generic;
using UFX.impl;

namespace TradingSystem.View
{
    public partial class ProductForm : Forms.DefaultForm
    {
        private const string GridId = "fundmanagement";
        private GridConfig _gridConfig = null;
        private AccountBLL _accountBLL = null;

        private SortableBindingList<Fund> _dataSource = new SortableBindingList<Fund>(new List<Fund>());

        public ProductForm()
            :base()
        {
            InitializeComponent();
        }

        public ProductForm(GridConfig gridConfig, UFXBLLManager bLLManager)
            : this()
        {
            _gridConfig = gridConfig;
            _accountBLL = bLLManager.AccountBLL;

            this.LoadControl += new FormLoadHandler(Form_LoadControl);
            this.LoadData += new FormLoadHandler(Form_LoadData);
        }

        private bool Form_LoadControl(object sender, object data)
        {
            //set the monitorGridView
            TSDataGridViewHelper.AddColumns(this.gridView, _gridConfig.GetGid(GridId));
            Dictionary<string, string> colDataMap = GridViewBindingHelper.GetPropertyBinding(typeof(Fund));
            TSDataGridViewHelper.SetDataBinding(this.gridView, colDataMap);

            this.gridView.DataSource = _dataSource;

            return true;
        }

        private bool Form_LoadData(object sender, object data)
        {
            _dataSource.Clear();

            var result = _accountBLL.QueryAccount();
            if (result != Model.ConnectionCode.Success)
            {
                return false;
            }

            var accounts = LoginManager.Instance.Accounts;
            foreach (var account in accounts)
            {
                Fund fund = new Fund 
                {
                    FundCode = account.AccountCode,
                    FundName = account.AccountName,
                    EAccountType = account.AccountType
                };

                _dataSource.Add(fund);
            }

            return true;
        }

    }
}
