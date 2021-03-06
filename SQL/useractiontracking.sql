use tradingsystem

if object_id('useractiontracking') is not null
drop table useractiontracking

create table useractiontracking(
	Id				int identity(1, 1) primary key
	,UserId			int not null
	,CreatedDate	datetime
	,Action			int			--Model/UsageTracking/ActionType.cs
	,ResourceType	int			--Model/Permission/ResourceType.cs
	,ResourceId		int			
	,Num			int
	,ActionStatus	int			--Model/UsageTracking/ActionStatus
	,Details		varchar(3000)
)


go
if exists (select name from sysobjects where name='procUserActionTrackingInsert')
drop proc procUserActionTrackingInsert

go
create proc procUserActionTrackingInsert(
	@UserId			int
	,@CreatedDate	datetime
	,@Action		int
	,@ResourceType	int
	,@ResourceId	int
	,@Num			int
	,@ActionStatus	int
	,@Details		varchar(3000)
)
as
begin
	declare @newid int

	insert into useractiontracking(
		UserId			
		,CreatedDate	
		,Action			
		,ResourceType	
		,ResourceId	
		,Num
		,ActionStatus	
		,Details		
	)
	values(
		@UserId			
		,@CreatedDate	
		,@Action			
		,@ResourceType	
		,@ResourceId	
		,@Num
		,@ActionStatus	
		,@Details		
	)

	set @newid = SCOPE_IDENTITY()
	return @newid
end

go
if exists (select name from sysobjects where name='procUserActionTrackingSelectByUser')
drop proc procUserActionTrackingSelectByUser

go
create proc procUserActionTrackingSelectByUser(
	@UserId int
)
as
begin
	select
		UserId
		,CreatedDate
		,Action
		,ResourceType
		,ResourceId
		,Num
		,ActionStatus
		,Details
	from useractiontracking
	where UserId=@UserId
end

go
if exists (select name from sysobjects where name='procUserActionTrackingSelectByUserPeriod')
drop proc procUserActionTrackingSelectByUserPeriod

go
create proc procUserActionTrackingSelectByUserPeriod(
	@UserId		int
	,@StartDate datetime
	,@EndDate	datetime
)
as
begin
	select
		UserId
		,CreatedDate
		,Action
		,ResourceType
		,ResourceId
		,Num
		,ActionStatus
		,Details
	from useractiontracking
	where UserId=@UserId
	and CreatedDate >= @StartDate
	and (@EndDate is null or CreatedDate <= @EndDate)
end

go
if exists (select name from sysobjects where name='procUserActionTrackingSelectByResource')
drop proc procUserActionTrackingSelectByResource

go
create proc procUserActionTrackingSelectByResource(
	@ResourceId		int
	,@ResourceType	int
)
as
begin
	select
		UserId
		,CreatedDate
		,Action
		,ResourceType
		,ResourceId
		,Num
		,ActionStatus
		,Details
	from useractiontracking
	where ResourceId=@ResourceId
	and ResourceType=@ResourceType
end