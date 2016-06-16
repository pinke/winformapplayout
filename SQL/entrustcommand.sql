use tradingsystem

--==通过交易系统委托之后，将委托指令添加到本表，由于可以分多次进行委托
if object_id('entrustcommand') is not null
drop table entrustcommand

create table entrustcommand(
	SubmitId		int identity(1, 1) primary key
	,CommandId		int not null
	,Copies			int			--指令份数
	,EntrustNo		int			--委托之后，服务器返回的委托号
	,BatchNo		int			--委托之后，服务器返回的批号
	,EntrustStatus	int
	,DealStatus		int
	,CreatedDate	datetime
	,ModifiedDate	datetime
)

go
if exists (select name from sysobjects where name='procEntrustCommandInsert')
drop proc procEntrustCommandInsert

go
create proc procEntrustCommandInsert(
	@CommandId		int
	,@Copies		int
	,@CreatedDate	datetime
)
as
begin
	declare @newid int
	insert into entrustcommand(
		CommandId
		,Copies
		,EntrustNo
		,BatchNo
		,EntrustStatus
		,DealStatus
		,CreatedDate
	)
	values(
		@CommandId
		,@Copies
		,-1
		,-1
		,0
		,1
		,@CreatedDate
	)

	set @newid = SCOPE_IDENTITY()
	return @newid
end

go
if exists (select name from sysobjects where name='procEntrustCommandUpdate')
drop proc procEntrustCommandUpdate

go
create proc procEntrustCommandUpdate(
	@SubmitId		int
	,@EntrustNo		int
	,@BatchNo		int
	,@EntrustStatus	int
	,@DealStatus	int
	,@ModifiedDate	datetime
)
as
begin
	update entrustcommand
	set EntrustNo		= @EntrustNo
		,BatchNo		= @BatchNo
		,EntrustStatus	= @EntrustStatus
		,DealStatus		= @DealStatus
		,ModifiedDate	= @ModifiedDate
	where SubmitId=@SubmitId
end


go
if exists (select name from sysobjects where name='procEntrustCommandUpdateDealStatus')
drop proc procEntrustCommandUpdateDealStatus

go
create proc procEntrustCommandUpdateDealStatus(
	@SubmitId			int
	,@DealStatus		int
	,@ModifiedDate		datetime
)
as
begin
	--declare @newid int
	update entrustcommand
	set DealStatus		= @DealStatus
		,ModifiedDate	= @ModifiedDate
	where SubmitId=@SubmitId
end


go
if exists (select name from sysobjects where name='procEntrustCommandUpdateEntrustStatus')
drop proc procEntrustCommandUpdateEntrustStatus

go
create proc procEntrustCommandUpdateEntrustStatus(
	@SubmitId			int
	,@EntrustStatus		int
	,@ModifiedDate		datetime
)
as
begin
	--declare @newid int
	update entrustcommand
	set EntrustStatus	= @EntrustStatus
		,ModifiedDate	= @ModifiedDate
	where SubmitId=@SubmitId
end

go
if exists (select name from sysobjects where name='procEntrustCommandUpdateCancel')
drop proc procEntrustCommandUpdateCancel

go
create proc procEntrustCommandUpdateCancel(
	@CommandId			int
	,@ModifiedDate		datetime
)
as
begin
	--declare @newid int
	update entrustcommand
	set EntrustStatus	= 10
		,ModifiedDate	= @ModifiedDate
	where CommandId=@CommandId
		and DealStatus = 1		--未成交
		and (EntrustStatus = 0	--提交到数据库
		or EntrustStatus = 1	--提交到UFX
		or EntrustStatus = 2	--未执行
		or EntrustStatus = 3	--部分执行
		or EntrustStatus = 4)	--已完成
end

go
if exists (select name from sysobjects where name='procEntrustCommandDeleteBySubmitId')
drop proc procEntrustCommandDeleteBySubmitId

go
create proc procEntrustCommandDeleteBySubmitId(
	@SubmitId		int
)
as
begin
	delete from entrustcommand
	where SubmitId=@SubmitId
end

go
if exists (select name from sysobjects where name='procEntrustCommandDeleteByCommandId')
drop proc procEntrustCommandDeleteByCommandId

go
create proc procEntrustCommandDeleteByCommandId(
	@CommandId		int
)
as
begin
	delete from entrustcommand
	where CommandId=@CommandId
end

go
if exists (select name from sysobjects where name='procEntrustCommandDeleteByCommandIdEntrustStatus')
drop proc procEntrustCommandDeleteByCommandIdEntrustStatus

go
create proc procEntrustCommandDeleteByCommandIdEntrustStatus(
	@CommandId		int
	,@EntrustStatus	int
)
as
begin
	delete from entrustcommand
	where CommandId=@CommandId and EntrustStatus=@EntrustStatus
end

go
if exists (select name from sysobjects where name='procEntrustCommandSelectBySubmitId')
drop proc procEntrustCommandSelectBySubmitId

go
create proc procEntrustCommandSelectBySubmitId(
	@SubmitId		int
)
as
begin
	select SubmitId
		  ,CommandId
		  ,Copies
		  ,EntrustNo
		  ,BatchNo
		  ,EntrustStatus
		  ,DealStatus
		  ,CreatedDate
		  ,ModifiedDate
	from entrustcommand
	where SubmitId=@SubmitId
end

go
if exists (select name from sysobjects where name='procEntrustCommandSelectByCommandId')
drop proc procEntrustCommandSelectByCommandId

go
create proc procEntrustCommandSelectByCommandId(
	@CommandId		int
)
as
begin
	select SubmitId
		  ,CommandId
		  ,Copies
		  ,EntrustNo
		  ,BatchNo
		  ,EntrustStatus
		  ,DealStatus
		  ,CreatedDate
		  ,ModifiedDate
	from entrustcommand
	where CommandId=@CommandId
end


go
if exists (select name from sysobjects where name='procEntrustCommandSelectAll')
drop proc procEntrustCommandSelectAll

go
create proc procEntrustCommandSelectAll
as
begin
	select SubmitId
		  ,CommandId
		  ,Copies
		  ,EntrustNo
		  ,BatchNo
		  ,EntrustStatus
		  ,DealStatus
		  ,CreatedDate
		  ,ModifiedDate
	from entrustcommand
end


go
if exists (select name from sysobjects where name='procEntrustCommandSelectByCommandIdEntrustStatus')
drop proc procEntrustCommandSelectByCommandIdEntrustStatus

go
create proc procEntrustCommandSelectByCommandIdEntrustStatus(
	@CommandId		int
	,@EntrustStatus	int
)
as
begin
	select SubmitId
		  ,CommandId
		  ,Copies
		  ,EntrustNo
		  ,BatchNo
		  ,EntrustStatus
		  ,DealStatus
		  ,CreatedDate
		  ,ModifiedDate
	from entrustcommand
	where CommandId = @CommandId 
		and EntrustStatus=@EntrustStatus
end

go
if exists (select name from sysobjects where name='procEntrustCommandSelectCancel')
drop proc procEntrustCommandSelectCancel

go
create proc procEntrustCommandSelectCancel(
	@CommandId int
)
as
begin
	select SubmitId
		  ,CommandId
		  ,Copies
		  ,EntrustNo
		  ,BatchNo
		  ,EntrustStatus
		  ,DealStatus
		  ,CreatedDate
		  ,ModifiedDate
	from entrustcommand
	where DealStatus=1		--未成交
		and EntrustStatus != 10	--撤单
		and EntrustStatus != 11 --撤单到UFX
		and EntrustStatus != 12 --撤单成功
end

go
if exists (select name from sysobjects where name='procEntrustCommandSelectCancelRedo')
drop proc procEntrustCommandSelectCancelRedo

go
create proc procEntrustCommandSelectCancelRedo(
	@CommandId int
)
as
begin
	select SubmitId
		  ,CommandId
		  ,Copies
		  ,EntrustNo
		  ,BatchNo
		  ,EntrustStatus
		  ,DealStatus
		  ,CreatedDate
		  ,ModifiedDate
	from entrustcommand
	where (DealStatus = 1 		--未成交
		or DealStatus = 2)		--部分成交
		and EntrustStatus = 5	--已完成委托
end