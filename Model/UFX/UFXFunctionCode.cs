﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.UFX
{
    public enum UFXFunctionCode
    {
        //心跳
        HeartBeat = 10000,

        //登录
        Login = 10001,

        //退出登录
        Logout = 10002,

        //内存数据查询
        //QuerymemoryData = 10009,

        //账户查询
        QueryAccount = 30001,

        //资产单元查询
        QueryAssetUnit = 30002,

        //组合查询
        QueryPortfolio = 30003,

        //交易股东查询
        QueryHolder = 30004,

        //证券持仓查询(仅能查询股票)
        QuerySecurityHolding = 31001,

        //期货持仓查询
        QueryFutureHolding = 31003,

        //期货持仓明细查询
        QueryFutureDetailHolding = 31013,

        //多业务持仓查询（现货、期货、期权）
        QueryMultipleHolding = 31005,

        //证券委托查询（当日）
        QuerySecurityEntrust = 32001,

        //期货委托查询（当日）
        QueryFuturesEntrust = 32003,

        //证券历史委托查询（历史）
        QuerySecurityEntrustHistorical = 32101,

        //期货历史委托查询（历史）
        QueryFuturesEntrustHistorical = 32103,

        //证券成交查询（当日）
        QuerySecurityDeal = 33001,

        //证券成交查询（历史）
        QuerySecurityDealHistorical = 33101,

        //期货成交查询（当日）
        QueryFuturesDeal = 33003,

        //期货成交查询（历史）
        QueryFuturesDealHistorical = 33103,

        //账户资金查询
        QueryAccountMoney = 34001,

        //组合持仓查询
        QueryHolding = 35001,

        //沪深交易所资金查询
        QueryMoney = 35004,

        //可用期货保证金查询
        QueryFutureMargin = 35005,

        //当日委托查询
        QueryEntrust = 35006,

        //当日成交查询
        QueryDeal = 35007,

        //期权可用保证金查询
        QueryOptionMargin = 35008,

        //套利 现货模板信息查询
        QueryTradingInstance1 = 70002,

        //策略 现货模板信息查询
        QuerySpotTemplate = 70004,

        //策略 现货模板成份股信息查询
        QuerySpotTemplateStock = 70007,

        //监控单元查询
        QueryMonitorItem = 70014,

        //策略 交易实例查询
        QueryTradingInstance = 71002,

        //策略 账户组资金查询
        QueryAccountGroupMoney = 71003,


        //策略 实例篮子委托
        EntrustInstanceBasket = 71005,

        //策略 实例委托查询
        QueryEntrustInstance = 71006,

        //策略 实例成交查询
        QueryDealInstance = 71007,
        

        //沪深竞价系统委托
        Entrust = 91001,

        //沪深ETF基金申赎委托
        EntrustETF = 91003,

        //期货委托
        EntrustFutures = 91004,

        //篮子委托
        EntrustBasket = 91090,  

        //个股期权篮子委托
        EntrustStockOptionBasket = 91091,

        //委托撤销
        Withdraw = 91101,

        //撤销篮子委托
        WithdrawBasket = 91102,

        //消息中心心跳
        MCHeartBeat = 620000,

        //收到订阅应答
        MCRecvSubscribe = 620001,

        //收到取消订阅应答
        MCRecvUnsubscribe = 620002,
    }
}
