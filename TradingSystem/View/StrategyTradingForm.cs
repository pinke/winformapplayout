﻿using Config;
using Controls.Entity;
using Controls.GridView;
using DBAccess;
using Forms;
using Model.UI;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using Model.SecurityInfo;
using Quote;
using Model.config;
using Util;
using BLL.SecurityInfo;
using TradingSystem.Dialog;
using TradingSystem.TradeUtil;
using BLL.Entrust;
using Model.EnumType;
using Model.Binding.BindingUtil;
using BLL.UFX.impl;

namespace TradingSystem.View
{
    public partial class StrategyTradingForm : Forms.BaseForm
    {
        private const string GridCmdTradingId = "cmdtrading";
        private const string GridEntrustFlowId = "entrustflow";
        private const string GridDealFlowId = "dealflow";
        private const string GridCmdSecurityId = "cmdsecurity";
        private const string GridBuySellId = "buysell";

        //private TradingInstanceDAO _tradeInstdao = new TradingInstanceDAO();
        private TradingCommandDAO _tradecmddao = new TradingCommandDAO();
        private TradingCommandSecurityDAO _tradecmdsecudao = new TradingCommandSecurityDAO();
        private EntrustCommandDAO _entrustcmddao = new EntrustCommandDAO();
        private EntrustSecurityDAO _entrustsecudao = new EntrustSecurityDAO();
        private EntrustDAO _entrustdao = new EntrustDAO();

        private EntrustBLL _entrustBLL = new EntrustBLL();

        private SortableBindingList<TradingCommandItem> _cmdDataSource = new SortableBindingList<TradingCommandItem>(new List<TradingCommandItem>());
        private SortableBindingList<EntrustFlowItem> _efDataSource = new SortableBindingList<EntrustFlowItem>(new List<EntrustFlowItem>());
        private SortableBindingList<DealFlowItem> _dfDataSource = new SortableBindingList<DealFlowItem>(new List<DealFlowItem>());
        private SortableBindingList<EntrustItem> _eiDataSource = new SortableBindingList<EntrustItem>(new List<EntrustItem>());
        private SortableBindingList<CommandSecurityItem> _secuDataSource = new SortableBindingList<CommandSecurityItem>(new List<CommandSecurityItem>());

        GridConfig _gridConfig;
        public StrategyTradingForm()
            :base()
        {
            InitializeComponent();
        }

        public StrategyTradingForm(GridConfig gridConfig)
            :this()
        {
            _gridConfig = gridConfig;

            LoadControl += new FormLoadHandler(Form_LoadControl);
            LoadData += new FormLoadHandler(Form_LoadData);

            tabParentMain.SelectedIndexChanged += new EventHandler(TabControl_Parent_SelectedIndexChanged);
            tabChildSecurity.SelectedIndexChanged += new EventHandler(TabControl_Child_SelectedIndexChanged);

            this.cmdGridView.UpdateRelatedDataGridHandler += new UpdateRelatedDataGrid(GridView_Command_UpdateRelatedDataGridHandler);
            this.bsGridView.UpdateRelatedDataGridHandler += new UpdateRelatedDataGrid(GridView_BuySell_UpdateRelatedDataGridHandler);

            //Refresh
            this.tsbRefresh.Click += new EventHandler(ToolStripButton_Command_Refresh);
            this.tsbefRefresh.Click += new EventHandler(ToolStripButton_EntrustFlow_Refresh);
            this.tsbdfRefresh.Click += new EventHandler(ToolStripButton_DealFlow_Refresh);

            //cancel
            this.tsbCancel.Click += new EventHandler(ToolStripButton_Command_Cancel);

            this.tsbCancelRedo.Click += new EventHandler(ToolStripButton_Command_CancelRedo);

            this.tsbCancelAdd.Click += new EventHandler(ToolStripButton_Command_CancelAdd);

            //Calculate
            this.btnCalc.Click += new EventHandler(Button_Calculate_Click);

            this.btnEntrust.Click += new EventHandler(Button_Entrust_Click);

            //select/unselect
            this.btnCmdSelect.Click += new EventHandler(Button_Command_Select_Click);
            this.btnCmdUnSelect.Click += new EventHandler(Button_Command_UnSelect_Click);

            this.btnSecuSelect.Click += new EventHandler(Button_Security_Select_Click);
            this.btnSecuUnSelect.Click += new EventHandler(Button_Security_UnSelect_Click);

            this.cbSpotBuyPrice.SelectedIndexChanged += new EventHandler(ComboBox_PriceType_SelectedIndexChange);
            this.cbSpotSellPrice.SelectedIndexChanged += new EventHandler(ComboBox_PriceType_SelectedIndexChange);
            this.cbFuturesBuyPrice.SelectedIndexChanged += new EventHandler(ComboBox_PriceType_SelectedIndexChange);
            this.cbFuturesSellPrice.SelectedIndexChanged += new EventHandler(ComboBox_PriceType_SelectedIndexChange);
        }

        #region Command panel cancel/cancelappend/canceladd click event

        private void ToolStripButton_Command_Cancel(object sender, EventArgs e)
        {
            var selectCmdItems = _cmdDataSource.Where(p => p.Selection);
            if (selectCmdItems != null && selectCmdItems.Count() > 0)
            {
                foreach (var cmdItem in selectCmdItems)
                {
                    GridView_Command_Cancel(cmdItem);
                }
            }
        }

        private void GridView_Command_Cancel(TradingCommandItem cmdItem)
        {
            int retCancel = _entrustBLL.CancelOne(cmdItem);

            //int retCancel = _entrustsecudao.UpdateCancel(cmdItem.CommandId);
            //if (retCancel <= 0)
            //{
            //    //TODO: fail to cancel
            //}
            //else
            //{
            //    retCancel = _entrustcmddao.UpdateCancel(cmdItem.CommandId);
            //}

            //var cancelCmdItems = _entrustcmddao.GetByEntrustStatus(cmdItem.CommandId, EntrustStatus.CancelToDB);
            //if (cancelCmdItems != null && cancelCmdItems.Count() > 0)
            //{
            //    cancelCmdItems.ForEach(p =>
            //    {
            //        //TODO: call UFX to cancel
            //        //.....

            //        //Update the EntrustStatus in db table
            //        var cancelSecuItems = _entrustsecudao.GetByEntrustStatus(p.SubmitId, p.CommandId, EntrustStatus.CancelToDB);
            //        if (cancelSecuItems != null && cancelSecuItems.Count() > 0)
            //        {
            //            cancelSecuItems.ForEach(s =>
            //            {
            //                _entrustsecudao.UpdateEntrustStatus(s, EntrustStatus.CancelToUFX);
            //            });
            //        }

            //        _entrustdao.UpdateOneEntrustStatus(p.SubmitId, EntrustStatus.CancelToUFX);
            //        //TODO: callback from UFX to update the CancelStatus to Fail or Success

            //        //Change the status to CancelSuccess after UFX callback/response
            //        cancelSecuItems = _entrustsecudao.GetByEntrustStatus(p.SubmitId, p.CommandId, EntrustStatus.CancelToUFX);
            //        if (cancelSecuItems != null && cancelSecuItems.Count() > 0)
            //        {
            //            cancelSecuItems.ForEach(s =>
            //            {
            //                _entrustsecudao.UpdateEntrustStatus(s, EntrustStatus.CancelSuccess);
            //            });
            //        }
            //        _entrustdao.UpdateOneEntrustStatus(p.SubmitId, EntrustStatus.CancelSuccess);

            //        //Update the tradingcommand table TargetNum
            //        cmdItem.TargetNum -= p.Copies;
            //        _tradecmddao.UpdateTargetNum(cmdItem);
            //    });
            //}

            this.cmdGridView.Invalidate();
        }

        private void ToolStripButton_Command_CancelRedo(object sender, EventArgs e)
        {
            var selectCmdItems = _cmdDataSource.Where(p => p.Selection).ToList();
            if (selectCmdItems == null || selectCmdItems.Count() == 0)
            {
                return;
            }

            var form = new CancelRedoDialog(_gridConfig);
            form.Owner = this;
            form.OnLoadControl(form, null);
            form.OnLoadData(form, selectCmdItems);
            form.SaveData += new FormLoadHandler(Dialog_CancelRedoDialog_SaveData);
            form.ShowDialog();
        }

        private bool Dialog_CancelRedoDialog_SaveData(object sender, object data)
        {
            throw new NotImplementedException();
        }

        private void ToolStripButton_Command_CancelAdd(object sender, EventArgs e)
        {
            var selectCmdItems = _cmdDataSource.Where(p => p.Selection);
            //撤销本次计算结果，仅影响到指令证券中的价格类型、本次委托数量、目标数量、指令份数
            if (selectCmdItems != null && selectCmdItems.Count() > 0)
            {
                foreach (var cmdItem in selectCmdItems)
                {
                    GridView_Command_CancelAdd(cmdItem);
                }
            }

            //Update the GridView
            this.securityGridView.Invalidate();
        }

        private void GridView_Command_CancelAdd(TradingCommandItem cmdItem)
        {
            //Reset some columns
            var secuItems = _secuDataSource.Where(p => p.CommandId == cmdItem.CommandId);
            if (secuItems != null && secuItems.Count() > 0)
            {

                foreach (var secuItem in secuItems)
                {
                    secuItem.Selection = true;
                    secuItem.CommandCopies = cmdItem.CommandNum;
                    secuItem.TargetCopies = cmdItem.TargetNum;
                    secuItem.TargetAmount = secuItem.TargetCopies * secuItem.WeightAmount;
                    secuItem.WaitAmount = secuItem.TargetCopies * secuItem.WeightAmount;
                    secuItem.EPriceType = PriceType.None;
                    secuItem.EntrustPrice = 0.0f;
                    secuItem.ThisEntrustAmount = 0;
                }
            }
        }

        #endregion

        #region price type change

        private void ComboBox_PriceType_SelectedIndexChange(object sender, EventArgs e)
        {
            if (sender == null || !(sender is ComboBox))
                return;
            ComboBox cb = sender as ComboBox;

            PriceType priceType = PriceTypeHelper.GetPriceType(cb);
            EntrustDirection direction = EntrustDirection.BuySpot;
            SecurityType secuType = SecurityType.All;
            switch (cb.Name)
            {
                case "cbSpotBuyPrice":
                    { 
                        direction = EntrustDirection.BuySpot;
                        secuType = SecurityType.Stock;
                    }
                    break;
                case "cbSpotSellPrice":
                    {
                        direction = EntrustDirection.SellSpot;
                        secuType = SecurityType.Stock;
                    }
                    break;
                case "cbFuturesBuyPrice":
                    {
                        direction = EntrustDirection.BuyClose;
                        secuType = SecurityType.Futures;
                    }
                    break;
                case "cbFuturesSellPrice":
                    {
                        direction = EntrustDirection.SellOpen;
                        secuType = SecurityType.Futures;
                    }
                    break;
                default:
                    break;
            }

            var items = _secuDataSource.Where(p => p.SecuType == secuType && p.EDirection == direction).ToList();
            if (items != null && items.Count > 0)
            {
                foreach (var item in items)
                {
                    item.EPriceType = priceType;
                }
            }

            this.securityGridView.Invalidate();
        }

        #endregion

        #region GridView UpdateRelatedDataGridHandler

        private void GridView_Command_UpdateRelatedDataGridHandler(UpdateDirection direction, int rowIndex, int columnIndex)
        {
            if (rowIndex < 0 || rowIndex >= _cmdDataSource.Count)
                return;

            TradingCommandItem cmdItem = _cmdDataSource[rowIndex];

            switch (direction)
            {
                case UpdateDirection.Select:
                    {
                        GridView_Command_Select(cmdItem);
                    }
                    break;
                case UpdateDirection.UnSelect:
                    {
                        GridView_Command_UnSelect(cmdItem);
                    }
                    break;
                case UpdateDirection.Increase:
                    {
                        
                    }
                    break;
                case UpdateDirection.Decrease:
                    {
                        
                    }
                    break;
            }
        }

        private void GridView_BuySell_UpdateRelatedDataGridHandler(UpdateDirection direction, int rowIndex, int columnIndex)
        {
            if (rowIndex < 0 || rowIndex >= _eiDataSource.Count)
                return;

            EntrustItem eiItem = _eiDataSource[rowIndex];
            switch (direction)
            {
                case UpdateDirection.Select:
                    {
                    }
                    break;
                case UpdateDirection.UnSelect:
                    {
                    }
                    break;
                case UpdateDirection.Increase:
                    { 
                        
                    }
                    break;
                case UpdateDirection.Decrease:
                    break;
                default:
                    break;
            }
        }

        private void GridView_Command_Select(TradingCommandItem cmdItem)
        {
            var spotBuyPrice = PriceTypeHelper.GetPriceType(this.cbSpotBuyPrice);
            var spotSellPrice = PriceTypeHelper.GetPriceType(this.cbSpotSellPrice);
            var futuBuyPrice = PriceTypeHelper.GetPriceType(this.cbFuturesBuyPrice);
            var futuSellPrice = PriceTypeHelper.GetPriceType(this.cbFuturesSellPrice);

            //Add into two gridview: CommandSecurity and entrust
            var secuItems = _secuDataSource.Where(p => p.CommandId == cmdItem.CommandId);
            if (secuItems == null || secuItems.Count() == 0)
            {
                secuItems = _tradecmdsecudao.Get(cmdItem.CommandId);
                if (secuItems != null)
                {
                    foreach (var secuItem in secuItems)
                    {
                        secuItem.Selection = true;
                        secuItem.CommandCopies = cmdItem.CommandNum;
                        secuItem.TargetCopies = cmdItem.TargetNum;
                        secuItem.TargetAmount = secuItem.TargetCopies * secuItem.WeightAmount;
                        secuItem.FundCode = cmdItem.FundCode;
                        secuItem.PortfolioName = cmdItem.PortfolioName;

                        var findItem = SecurityInfoManager.Instance.Get(secuItem.SecuCode);
                        if (findItem != null)
                        {
                            secuItem.SecuName = findItem.SecuName;
                        }

                        switch (secuItem.EDirection)
                        {
                            case EntrustDirection.BuySpot:
                                {
                                    secuItem.EPriceType = spotBuyPrice;
                                }
                                break;
                            case EntrustDirection.SellSpot:
                                {
                                    secuItem.EPriceType = spotSellPrice;
                                }
                                break;
                            case EntrustDirection.SellOpen:
                                {
                                    secuItem.EPriceType = futuSellPrice;
                                }
                                break;
                            case EntrustDirection.BuyClose:
                                {
                                    secuItem.EPriceType = futuBuyPrice;
                                }
                                break;
                            default:
                                break;
                        }


                        _secuDataSource.Add(secuItem);
                    }
                }
            }

            //Add into buy/sell grid view
            var entrustItems = _eiDataSource.Where(p => p.CommandNo == cmdItem.CommandId);
            if (entrustItems == null || entrustItems.Count() == 0)
            {
                var entrustItem = new EntrustItem
                {
                    CommandNo = cmdItem.CommandId,
                    Selection = true
                };

                _eiDataSource.Add(entrustItem);
            }
        }

        private void GridView_Command_UnSelect(TradingCommandItem cmdItem)
        {
            //Remove from Security GridView
            var secuItems = _secuDataSource.Where(p => p.CommandId == cmdItem.CommandId).ToList();
            if (secuItems != null && secuItems.Count() > 0)
            {
                foreach (var secuItem in secuItems)
                {
                    _secuDataSource.Remove(secuItem);
                }
            }

            //Remove from two GridView:
            var entrustItems = _eiDataSource.Where(p => p.CommandNo == cmdItem.CommandId).ToList();
            if (entrustItems != null && entrustItems.Count() > 0)
            {
                foreach (var item in entrustItems)
                {
                    _eiDataSource.Remove(item);
                }
            }
        }

        #endregion

        #region form loading

        private bool Form_LoadControl(object sender, object data)
        {
            //Load Command Trading
            TSDataGridViewHelper.AddColumns(this.cmdGridView, _gridConfig.GetGid(GridCmdTradingId));
            Dictionary<string, string> cmdColDataMap = GridViewBindingHelper.GetPropertyBinding(typeof(TradingCommandItem));
            TSDataGridViewHelper.SetDataBinding(this.cmdGridView, cmdColDataMap);           

            //Load EntrustFlow gridview
            TSDataGridViewHelper.AddColumns(this.efGridView, _gridConfig.GetGid(GridEntrustFlowId));
            Dictionary<string, string> efColDataMap = GridViewBindingHelper.GetPropertyBinding(typeof(EntrustFlowItem));
            TSDataGridViewHelper.SetDataBinding(this.efGridView, efColDataMap);

            //Load DealFlow gridview
            TSDataGridViewHelper.AddColumns(this.dfGridView, _gridConfig.GetGid(GridDealFlowId));
            Dictionary<string, string> dfColDataMap = GridViewBindingHelper.GetPropertyBinding(typeof(DealFlowItem));
            TSDataGridViewHelper.SetDataBinding(this.dfGridView, dfColDataMap);

            //Load Security gridview
            TSDataGridViewHelper.AddColumns(this.securityGridView, _gridConfig.GetGid(GridCmdSecurityId));
            Dictionary<string, string> secuColDataMap = GridViewBindingHelper.GetPropertyBinding(typeof(CommandSecurityItem));
            TSDataGridViewHelper.SetDataBinding(this.securityGridView, secuColDataMap);

            //Load Entrust gridview
            TSDataGridViewHelper.AddColumns(this.bsGridView, _gridConfig.GetGid(GridBuySellId));
            Dictionary<string, string> bsColDataMap = GridViewBindingHelper.GetPropertyBinding(typeof(EntrustItem));
            TSDataGridViewHelper.SetDataBinding(this.bsGridView, bsColDataMap); 

            //Load combobox
            LoadEntrustControl();

            //Binding data
            this.cmdGridView.DataSource = _cmdDataSource;
            this.efGridView.DataSource = _efDataSource;
            this.dfGridView.DataSource = _dfDataSource;
            this.bsGridView.DataSource = _eiDataSource;
            this.securityGridView.DataSource = _secuDataSource;

            return true;
        }

        private void LoadEntrustControl()
        {
            var spotPrices = ConfigManager.Instance.GetComboConfig().GetComboOption("spotprice");
            ComboBoxUtil.SetComboBox(this.cbSpotBuyPrice, spotPrices);

            var spotSellPrices = new ComboOption 
            {
                Name = spotPrices.Name,
                Selected = spotPrices.Selected,
                Items = spotPrices.Items.OrderBy(p => p.Order2).ToList()
            };
            ComboBoxUtil.SetComboBox(this.cbSpotSellPrice, spotSellPrices);

            var futurePrice = ConfigManager.Instance.GetComboConfig().GetComboOption("futureprice");
            ComboBoxUtil.SetComboBox(this.cbFuturesBuyPrice, futurePrice);

            var futureSellPrices = new ComboOption
            {
                Name = futurePrice.Name,
                Selected = futurePrice.Selected,
                Items = futurePrice.Items.OrderBy(p => p.Order2).ToList()
            };

            ComboBoxUtil.SetComboBox(this.cbFuturesSellPrice, futureSellPrices);
        }

        #endregion

        #region tabcontrol index changed

        private void TabControl_Parent_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabControl tc = (TabControl)sender;
            if (tc == null)
                return;

            string selectTabName = tc.SelectedTab.Name;
            if (selectTabName == this.tpEntrustFlow.Name)
            {
                this.tpCmdEntrustFlow.Controls.Clear();
                this.efMainPanel.Controls.Clear();
                this.efMainPanel.Controls.Add(this.efGridView);
            }
            else if (selectTabName == this.tpDealFlow.Name)
            {
                this.tpCmdDealFlow.Controls.Clear();
                this.dfMainPanel.Controls.Clear();
                this.dfMainPanel.Controls.Add(this.dfGridView);
            }
            else if (selectTabName == tpCmdTrading.Name)
            {
                SwitchChildTabPage(this.tabChildSecurity.SelectedTab.Name);
            }
        }

        private void SwitchChildTabPage(string selectTabName)
        {
            if (selectTabName == this.tpCmdSecurity.Name)
            {
                //do nothing
            }
            else if (selectTabName == this.tpCmdEntrustFlow.Name)
            {
                this.efMainPanel.Controls.Clear();
                this.tpCmdEntrustFlow.Controls.Clear();
                this.tpCmdEntrustFlow.Controls.Add(this.efGridView);
            }
            else if (selectTabName == this.tpCmdDealFlow.Name)
            {
                this.dfMainPanel.Controls.Clear();
                this.tpCmdDealFlow.Controls.Clear();
                this.tpCmdDealFlow.Controls.Add(this.dfGridView);
            }
        }

        private void TabControl_Child_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabControl tc = (TabControl)sender;
            if (tc == null)
                return;

            string selectTabName = tc.SelectedTab.Name;
            SwitchChildTabPage(selectTabName);
        }

        #endregion

        #region Load data
        private bool Form_LoadData(object sender, object data)
        {
            //Clear();
            
            //Load data here
            LoadDataTradeCommand();
            LoadDataEntrustFlow();
            LoadDataDealFlow();

            return true;
        }

        private void Clear()
        {
            _cmdDataSource.Clear();
            _secuDataSource.Clear();
            _efDataSource.Clear();
            _eiDataSource.Clear();
            _dfDataSource.Clear();
        }

        private bool LoadDataTradeCommand()
        {
            _cmdDataSource.Clear();
            _secuDataSource.Clear();
            _eiDataSource.Clear();

            var tradingcmds = _tradecmddao.Get(-1);
            if (tradingcmds != null)
            {
                foreach (var cmdItem in tradingcmds)
                {
                    _cmdDataSource.Add(cmdItem);
                }
            }

            return true;
        }

        private bool LoadDataEntrustFlow()
        {
            //var efItems = _entrustBLL.GetEntrustFlow();
            //if (efItems != null)
            //{
            //    foreach (var efItem in efItems)
            //    {
            //        _efDataSource.Add(efItem);
            //    }
            //}

            UFXQueryEntrustBLL queryBll = new UFXQueryEntrustBLL();
            var test = queryBll.QueryToday(new CallerCallback(LoadDataEntrustFlow2));

            return true;
        }

        private bool LoadDataDealFlow()
        {
            //var dfItems = _entrustBLL.GetDealFlow();
            //if (dfItems != null)
            //{
            //    foreach (var dfItem in dfItems)
            //    {
            //        _dfDataSource.Add(dfItem);
            //    }
            //}

            UFXQueryDealBLL queryDealBll = new UFXQueryDealBLL();
            var test = queryDealBll.QueryToday(new CallerCallback(LoadDataDealFlow2));

            return true;
        }

        private int LoadDataDealFlow2(CallerToken token, object data)
        {
            if (data == null || !(data is List<DealFlowItem>))
                return -1;
            var dfItems = data as List<DealFlowItem>;

            this.BeginInvoke(new Action(() =>
            {
                _efDataSource.Clear();

                dfItems.ForEach(p => _dfDataSource.Add(p));

                this.efGridView.Invalidate();
            }), null);
            return 1;
        }
        
        private int LoadDataEntrustFlow2(CallerToken token, object data)
        {
            if (data == null || !(data is List<EntrustFlowItem>))
                return -1;
            var efItems = data as List<EntrustFlowItem>;
            
            this.BeginInvoke(new Action(()=>
            {
                _efDataSource.Clear();

                efItems.ForEach(p => _efDataSource.Add(p));

                this.efGridView.Invalidate();
            }), null);
            return 1;
        }

        #endregion

        #region toolstrip refresh click event handler

        private void ToolStripButton_DealFlow_Refresh(object sender, EventArgs e)
        {
            //TODO: load the dealflow security
            LoadDataDealFlow();
        }

        private void ToolStripButton_EntrustFlow_Refresh(object sender, EventArgs e)
        {
            //TODO: load the entrustflow security
            LoadDataEntrustFlow();
        }

        private void ToolStripButton_Command_Refresh(object sender, EventArgs e)
        {
            Form_LoadData(this, null);
        }

        #endregion

        #region TradingCommand select/unselect click

        private void Button_Command_Select_Click(object sender, EventArgs e)
        {
            foreach (var cmdItem in _cmdDataSource)
            {
                cmdItem.Selection = true;

                GridView_Command_Select(cmdItem);
            }

            this.cmdGridView.InvalidateColumn(0);
            this.securityGridView.InvalidateColumn(0);
            this.bsGridView.InvalidateColumn(0);
        }

        private void Button_Command_UnSelect_Click(object sender, EventArgs e)
        {
            foreach (var cmdItem in _cmdDataSource)
            {
                cmdItem.Selection = false;

                GridView_Command_UnSelect(cmdItem);
            }

            this.cmdGridView.InvalidateColumn(0);
            this.securityGridView.InvalidateColumn(0);
            this.bsGridView.InvalidateColumn(0);
        }

        #endregion

        #region TradingCommand Security select/unselect

        private void Button_Security_Select_Click(object sender, EventArgs e)
        {
            foreach (var secuItem in _secuDataSource)
            {
                secuItem.Selection = true;
            }

            this.securityGridView.Invalidate();
        }

        private void Button_Security_UnSelect_Click(object sender, EventArgs e)
        {
            foreach (var secuItem in _secuDataSource)
            {
                secuItem.Selection = false;
            }

            this.securityGridView.Invalidate();
        }

        #endregion

        #region buy/sell button click event

        private void Button_Calculate_Click(object sender, EventArgs e)
        {
            //TODO:
            if (!ValidateCopies())
            {
                MessageBox.Show(this, "委托份数非法，请输入正确的委托份数！", "警告", MessageBoxButtons.OK);
                return;
            }

            //Get the price type
            PriceType spotBuyPrice = PriceTypeHelper.GetPriceType(this.cbSpotBuyPrice);
            PriceType spotSellPrice = PriceTypeHelper.GetPriceType(this.cbSpotSellPrice);
            PriceType futureBuyPrice = PriceTypeHelper.GetPriceType(this.cbFuturesBuyPrice);
            PriceType futureSellPrice = PriceTypeHelper.GetPriceType(this.cbFuturesSellPrice);

            //update each command item
            foreach (var eiItem in _eiDataSource)
            {
                var selCmdItem = _cmdDataSource.Single(p => p.CommandId == eiItem.CommandNo);
                if (selCmdItem != null)
                {
                    //selCmdItems[0].TargetNum = eiItem.Copies;
                    int thisCopies = eiItem.Copies;
                    int targetNum = selCmdItem.TargetNum + eiItem.Copies;

                    _secuDataSource.Where(p => p.CommandId == eiItem.CommandNo)
                        .ToList()
                        .ForEach(p => {
                            p.TargetCopies = targetNum;
                            p.TargetAmount = p.TargetCopies * p.WeightAmount;
                            p.ThisEntrustAmount = thisCopies * p.WeightAmount;
                            p.WaitAmount = p.TargetCopies * p.WeightAmount;

                            var direction = EntrustDirectionUtil.GetEntrustDirection(p.EntrustDirection);
                            switch (direction)
                            {
                                case EntrustDirection.BuySpot:
                                    {
                                        if (p.SecuType == SecurityType.Stock)
                                        {
                                            p.EPriceType = spotBuyPrice;
                                        }
                                    }
                                    break;
                                case EntrustDirection.SellSpot:
                                    {
                                        if (p.SecuType == SecurityType.Stock)
                                        {
                                            p.EPriceType = spotSellPrice;
                                        }
                                    }
                                    break;
                                case EntrustDirection.SellOpen:
                                    {
                                        if (p.SecuType == SecurityType.Futures)
                                        {
                                            p.EPriceType = futureSellPrice;
                                        }
                                    }
                                    break;
                                case EntrustDirection.BuyClose:
                                    {
                                        if (p.SecuType == SecurityType.Futures)
                                        {
                                            p.EPriceType = futureBuyPrice;
                                        } 
                                    }
                                    break;
                                default:
                                    break;
                            }
                        });
                }
            }

            //query the price and set it
            List<SecurityItem> secuList = new List<SecurityItem>();
            var uniqueSecuItems = _secuDataSource.GroupBy(p => p.SecuCode).Select(p => p.First());
            foreach (var secuItem in uniqueSecuItems)
            {
                var findItem = SecurityInfoManager.Instance.Get(secuItem.SecuCode, secuItem.SecuType);
                secuList.Add(findItem);
            }

            List<PriceType> priceTypes = new List<PriceType>() { spotBuyPrice, spotSellPrice, futureBuyPrice, futureSellPrice };

            QuoteCenter.Instance.Query(secuList, priceTypes);
            foreach (var secuItem in _secuDataSource)
            {
                var targetItem = secuList.Find(p => p.SecuCode.Equals(secuItem.SecuCode) && (p.SecuType == SecurityType.Stock || p.SecuType == SecurityType.Futures));
                var marketData = QuoteCenter.Instance.GetMarketData(targetItem);

                secuItem.LimitUpPrice = marketData.HighLimitPrice;
                secuItem.LimitDownPrice = marketData.LowLimitPrice;
                secuItem.SuspensionFlag = marketData.SuspendFlag.ToString();

                switch (secuItem.SecuType)
                {
                    case SecurityType.Stock:
                        {

                            if (secuItem.EDirection == EntrustDirection.BuySpot)
                            {
                                secuItem.EntrustPrice = GetPrice(spotBuyPrice, marketData);
                            }
                            else if (secuItem.EDirection == EntrustDirection.SellSpot)
                            {
                                secuItem.EntrustPrice = GetPrice(spotSellPrice, marketData);
                            }
                        }
                        break;
                    case SecurityType.Futures:
                        {
                            if (secuItem.EDirection == EntrustDirection.SellOpen)
                            {
                                secuItem.EntrustPrice = GetPrice(futureSellPrice, marketData);
                            }
                            else if (secuItem.EDirection == EntrustDirection.BuyClose)
                            {
                                secuItem.EntrustPrice = GetPrice(futureBuyPrice, marketData);
                            }
                        }
                        break;
                }
            }

            //refresh UI
            this.cmdGridView.Invalidate();
            this.bsGridView.Invalidate();
            this.securityGridView.Invalidate();
        }

        private void Button_Entrust_Click(object sender, EventArgs e)
        {
            //TODO:
            //Check the entrust security items of entrust item. Make sure there is security item selected
            var selectedEntrustItems = _eiDataSource.Where(p => p.Selection).ToList();
            if (selectedEntrustItems == null || selectedEntrustItems.Count == 0)
            {
                //TODO: show message
                MessageBox.Show(this, "请选择要委托的指令！", "警告", MessageBoxButtons.OK);
                return;
            }

            foreach (var eiItem in selectedEntrustItems)
            {
                int count = _secuDataSource.Count(p => p.Selection && p.CommandId == eiItem.CommandNo);
                if (count == 0)
                {
                    MessageBox.Show(this, "请确保委托指令中包含有效证券！", "警告", MessageBoxButtons.OK);
                    return;
                }
            }

            if (!ValidateEntrust())
            {
                MessageBox.Show(this, "请设置委托的证券数量和价格，证券数量需为正值，价格不能为零且需在最高最低价之间！", "警告", MessageBoxButtons.OK);
                return;
            }

            //submit each entrust item and each security in the entrustitem
            List<int> submitIds = new List<int>();
            //1. submit into database
            foreach (var eiItem in selectedEntrustItems)
            {
                if (eiItem.Copies <= 0)
                {
                    //TODO: please input copies
                    continue;
                }

                var cmdItem = _cmdDataSource.Single(p => p.CommandId == eiItem.CommandNo);
                if (cmdItem == null)
                {
                    //TODO: there is no CommandItem
                    continue;
                }

                //cmdItem.TargetNum += eiItem.Copies;
                int targetNum = cmdItem.TargetNum + eiItem.Copies;
                if (targetNum > cmdItem.CommandNum)
                { 
                    //TODO: there are no so many securities
                    continue;
                }

                EntrustCommandItem eciItem = new EntrustCommandItem
                {
                    CommandId = eiItem.CommandNo,
                    Copies = eiItem.Copies,
                };

                var entrustSecuItems = GetEntrustSecurityItems(-1, eiItem.CommandNo);
                //int submitRet = _entrustdao.Submit(eciItem, entrustSecuItems);
                int submitRet = _entrustBLL.SubmitOne(eciItem, entrustSecuItems);

                if (submitRet > 0)
                {
                    //success to submit into database
                    submitIds.Add(eciItem.SubmitId);

                    //update the TargetNum
                    cmdItem.TargetNum = targetNum;
                    int targetNumFlag = _tradecmddao.UpdateTargetNum(cmdItem);
                    if (targetNumFlag <= 0)
                    {
                        //TODO: failed to update TargetNum
                    }
                }
                else
                { 
                    //Fail to submit into database
                }
            }

            //update the UI
            nudCopies.Value = 1;
            this.cmdGridView.Invalidate();
            this.bsGridView.Invalidate();
        }

        private List<EntrustSecurityItem> GetEntrustSecurityItems(int submitId, int commandId)
        {
            List<EntrustSecurityItem> entrustSecuItems = new List<EntrustSecurityItem>();

            var secuItems = _secuDataSource.Where(p => p.Selection && p.CommandId == commandId).ToList();
            if (secuItems == null || secuItems.Count == 0)
            {
                return entrustSecuItems;
            }

            foreach (var secuItem in secuItems)
            {
                var priceType = PriceTypeHelper.GetPriceType(secuItem.PriceType);
                EntrustSecurityItem entrustSecurityItem = new EntrustSecurityItem
                {
                    SubmitId = submitId,
                    CommandId = commandId,
                    SecuCode = secuItem.SecuCode,
                    SecuType = secuItem.SecuType,
                    EntrustAmount = secuItem.ThisEntrustAmount,
                    EntrustPrice = secuItem.EntrustPrice,
                    EntrustDirection = secuItem.EDirection,
                    EntrustStatus = EntrustStatus.SubmitToDB,
                    PriceType = priceType,
                    EntrustDate = DateTime.Now,
                };

                var secuInfo = SecurityInfoManager.Instance.Get(secuItem.SecuCode, secuItem.SecuType);
                string exchangeCode = string.Empty;
                if (secuInfo != null)
                {
                    exchangeCode = secuInfo.ExchangeCode;
                }
                else
                {
                    exchangeCode = SecurityInfoHelper.GetExchangeCode(secuItem.SecuCode);
                }

                if (exchangeCode.Equals("SZSE"))
                {
                    entrustSecurityItem.EntrustPriceType = EntrustPriceType.FifthIsLeftOffSZ;
                }
                else if (exchangeCode.Equals("SSE"))
                {
                    entrustSecurityItem.EntrustPriceType = EntrustPriceType.FifthIsLeftOffSH;
                }
                else if (exchangeCode.Equals("CFFEX"))
                {
                    entrustSecurityItem.EntrustPriceType = EntrustPriceType.FifthIsLeftOffCFX;
                }
                else
                { 
                    //Do nothing
                }
                

                entrustSecuItems.Add(entrustSecurityItem);
            }

            return entrustSecuItems;
        }

        private bool ValidateCopies()
        {
            int copies = (int)nudCopies.Value;

            if (copies > 0)
            {
                _eiDataSource.Where(p => p.Selection).ToList().ForEach(p => p.Copies = copies);
            }
            else
            {
                foreach (var eiItem in _eiDataSource)
                {
                    var cmdItem = _cmdDataSource.Single(p => p.Selection && p.CommandId == eiItem.CommandNo);
                    if (cmdItem != null && cmdItem.CommandNum < eiItem.Copies)
                    {
                        return false;
                    }
                }
            }

            var selItems = _eiDataSource.Where(p => p.Selection && p.Copies == 0).ToList();
            if (selItems != null && selItems.Count > 0)
            {
                return false;
            }

            return true;
        }

        private bool ValidateEntrust()
        {
            var entrustSecuItems = _secuDataSource.Where(p => p.Selection).ToList();
            if (entrustSecuItems == null || entrustSecuItems.Count == 0)
            {
                //TODO: show message
                MessageBox.Show(this, "请选择要委托的证券！", "警告", MessageBoxButtons.OK);
                return false;
            }

            foreach (var entrustSecuItem in entrustSecuItems)
            {
                if (entrustSecuItem.ThisEntrustAmount <= 0
                    || entrustSecuItem.EntrustPrice < entrustSecuItem.LimitDownPrice
                    || entrustSecuItem.EntrustPrice > entrustSecuItem.LimitUpPrice
                    || entrustSecuItem.EntrustPrice <= 0.00001)
                {
                    return false;
                }
            }

            return true;
        }

        private double GetPrice(PriceType priceType, MarketData marketData)
        {
            double price = 0f;
            switch (priceType)
            { 
                case PriceType.Last:
                case PriceType.Arbitrary:
                case PriceType.Assign:
                case PriceType.Automatic:
                    price = marketData.CurrentPrice;
                    break;
                case PriceType.Sell1:
                    price = marketData.SellPrice1;
                    break;
                case PriceType.Sell2:
                    price = marketData.SellPrice2;
                    break;
                case PriceType.Sell3:
                    price = marketData.SellPrice3;
                    break;
                case PriceType.Sell4:
                    price = marketData.SellPrice4;
                    break;
                case PriceType.Sell5:
                case PriceType.Sell6:
                case PriceType.Sell7:
                case PriceType.Sell8:
                case PriceType.Sell9:
                case PriceType.Sell10:
                    price = marketData.SellPrice5;
                    break;
                case PriceType.Buy1:
                    price = marketData.BuyPrice1;
                    break;
                case PriceType.Buy2:
                    price = marketData.BuyPrice2;
                    break;
                case PriceType.Buy3:
                    price = marketData.BuyPrice3;
                    break;
                case PriceType.Buy4:
                    price = marketData.BuyPrice4;
                    break;
                case PriceType.Buy5:
                case PriceType.Buy6:
                case PriceType.Buy7:
                case PriceType.Buy8:
                case PriceType.Buy9:
                case PriceType.Buy10:
                    price = marketData.BuyPrice5;
                    break;
                default:
                    price = marketData.CurrentPrice;
                    break;
            }

            return price;
        }

        #endregion
    }
}
