﻿using BLL.Manager;
using Config;
using Forms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TradingSystem.View;

namespace TradingSystem.Controller
{
    public class FormManager
    {
        private readonly static FormManager _instance = new FormManager();

        //private static GridConfig _gridConfig;

        private FormManager()
        {
            Init();
        }

        static FormManager()
        { 
        
        }

        private void Init()
        {
            //_gridConfig = ConfigManager.Instance.GetGridConfig();
        }

        private Dictionary<string, Forms.BaseForm> _childFormMap = new Dictionary<string, Forms.BaseForm>();

        public static FormManager Instance { get { return _instance; } }

        public BaseForm ActiveForm(Form parentForm, Panel parentPanel, string formKey, GridConfig gridConfig, UFXBLLManager bLLManager)
        {
            Forms.BaseForm form = null;
            Type formType = null;
            bool hasGrid = false;
            bool needBLL = false;
            string json = string.Empty;
            if (_childFormMap.ContainsKey(formKey))
            {
                form = _childFormMap[formKey];
            }
            else
            {
                switch (formKey)
                {
                    case "cmdtrading":
                        {
                            formType = typeof(StrategyTradingForm);
                            hasGrid = true;
                        }
                        break;
                    case "open":
                        {
                            formType = typeof(OpenPositionForm);
                            hasGrid = true;
                        }
                        break;
                    case "close":
                        {
                            formType = typeof(ClosePositionForm);
                            hasGrid = true;
                        }
                        break;
                    case "commandmanager":
                        {
                            formType = typeof(CommandManagementForm);
                            hasGrid = true;
                        }
                        break;
                    case "monitorunit":
                        {
                            formType = typeof(MonitorUnitForm);
                            hasGrid = true;
                        }
                        break;
                    case "portfoliomaintain":
                        {
                            formType = typeof(PortfolioForm);
                            hasGrid = true;
                            needBLL = true;
                        }
                        break;
                    case "fundmanagement":
                        {
                            formType = typeof(ProductForm);
                            hasGrid = true;
                            needBLL = true;
                        }
                        break;
                    case "assetunitmanagement":
                        {
                            formType = typeof(AssetUnitForm);
                            hasGrid = true;
                            needBLL = true;
                        }
                        break;
                    case "spottemplate":
                        {
                            formType = typeof(SpotTemplateForm);
                            hasGrid = true;
                        }
                        break;
                    //case "currenttemplate":
                    //    {
                    //        formType = typeof(StockTemplateForm);
                    //        hasGrid = true;
                    //        //StockTemplateDAO _dbdao = new StockTemplateDAO();
                    //        //var items = _dbdao.GetTemplate(-1);
                    //        //json = JsonUtil.SerializeObject(items);
                    //    }
                    //    break;
                    case "historicaltemplate":
                        {
                            formType = typeof(HistSpotTemplateForm);
                            hasGrid = true;
                        }
                        break;
                    case "instancemanagement":
                        {
                            formType = typeof(InstanceManagementForm);
                            hasGrid = true;
                        }
                        break;
                    case "holdingtransfer":
                        {
                            formType = typeof(HoldingTransferedForm);
                            hasGrid = true;
                            needBLL = true;
                        }
                        break;
                    default:
                        break;
                }
            }

            if (formType != null && form == null)
            {
                if (needBLL && hasGrid)
                {
                    form = LoadForm(parentForm, formType, new object[] { gridConfig, bLLManager }, json);
                }
                else if (hasGrid)
                {
                    form = LoadForm(parentForm, formType, new object[] { gridConfig }, json);
                }
                else
                {
                    form = LoadForm(parentForm, formType, null, json);
                }

                _childFormMap[formKey] = form;
            }

            if (form != null)
            {
                ILoadData loadData = form as ILoadData;
                if (loadData != null)
                {
                    loadData.OnLoadData(form, null);
                }

                IFormActived formActived = form as IFormActived;
                if (formActived != null)
                {
                    //TODO: add the step to load data and refresh the child form
                    formActived.OnFormActived("");
                }

                form.MdiParent = parentForm;
                form.Parent = parentPanel;
                form.Dock = DockStyle.Fill;
                form.BringToFront();
                form.Show();
            }
            else
            {
                //default form
                throw new NotSupportedException("The type is not support!");
            }

            return form;
        }

        public BaseForm LoadForm(Form mainForm, Type formType, object[] constructorArgs, string json)
        {
            bool isFound = false;
            BaseForm childForm = null;
            foreach (Form form in mainForm.MdiChildren)
            {
                if (form is BaseForm)
                {
                    if (form.GetType() == formType)
                    {
                        isFound = true;
                        childForm = form as BaseForm;
                        break;
                    }
                }
            }

            if (!isFound)
            {
                if (constructorArgs != null && constructorArgs.Length > 0)
                {
                    childForm = (BaseForm)Activator.CreateInstance(formType, constructorArgs);
                }
                else
                {
                    childForm = (BaseForm)Activator.CreateInstance(formType);
                }

                ILoadControl loadControl = childForm as ILoadControl;
                if (loadControl != null)
                {
                    loadControl.OnLoadControl(childForm, null);
                }
                //childForm.MdiParent = mainForm;
                //childForm.Show();
            }

            //窗体激活的时候，传递对应的参数信息
            //ILoadFormActived formActived = childForm as ILoadFormActived;
            //if (formActived != null)
            //{
            //    formActived.OnLoadFormActived(json);
            //}

            //childForm.BringToFront();
            //childForm.Activate();

            return childForm;
        }
    }
}
