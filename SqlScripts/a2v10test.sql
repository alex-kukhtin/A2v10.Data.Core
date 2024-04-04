-- Copyright © 2008-2024 Oleksandr Kukhtin

/* 20240204-7358 */

use a2v10test;
go

------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.SCHEMATA where SCHEMA_NAME=N'a2test')
begin
	exec sp_executesql N'create schema a2test';
end
go
-----------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2test' and TABLE_NAME=N'Documents')
	create table a2test.Documents
	(
		Id bigint not null constraint PK_Documents primary key,
		[Name] nvarchar(255),
		[Date] datetime,
		[Sum] money,
		[Memo] nvarchar(255)
	);
go
-----------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2test' and TABLE_NAME=N'ScheduledCommands')
	create table a2test.ScheduledCommands
	(
		[Command] nvarchar(255),
		[Data] nvarchar(2500),
		UtcRunAt datetime null
	);
go
-----------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2test' and TABLE_NAME=N'Rows')
	create table a2test.[Rows]
	(
		Id bigint not null constraint PK_Rows primary key,
		Document bigint not null 
			constraint FK_Rows_Document_Documents references a2test.Documents(Id),
		[Memo] nvarchar(255),
		Qty float,
		Price money,
		[Sum] money
	);
go
------------------------------------------------
create or alter procedure a2test.[SimpleModel.Load]
	@TenantId int = null,
	@UserId bigint = null,
	@Id bigint = null
as
begin
	set nocount on;
	select [Model!TModel!Object] = null, [Id!!Id] = 123, [Name!!Name]='ObjectName', [Decimal] = cast(55.1234 as decimal(10, 5));
end
go
------------------------------------------------
create or alter procedure a2test.ArrayModel
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;
	select [Customers!TCustomer!Array] = null, [Id!!Id] = 123, [Name!!Name]='ObjectName', [Amount] = cast(55.1234 as decimal(10, 5));
end
go
------------------------------------------------
create or alter procedure a2test.ComplexModel
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;
	select [Document!TDocument!Object] = null, [Id!!Id] = 123, [No]='DocNo', [Date]=a2sys.fn_getCurrentDate(),
		[Agent!TAgent!RefId] = 512, [Company!TAgent!RefId] = 512,
		[Rows1!TRow!Array] = null, [Rows2!TRow!Array] = null;

	select [!TRow!Array] = null, [Id!!Id] = 78, [!TDocument.Rows1!ParentId] = 123, 
		[Product!TProduct!RefId] = 782,
		Qty=cast(4.0 as float), Price=cast(8 as money), [Sum] = cast(32.0 as money),
		[Series1!TSeries!Array] = null

	select [!TRow!Array] = null, [Id!!Id] = 79, [!TDocument.Rows2!ParentId] = 123, 
		[Product!TProduct!RefId] = 785,
		Qty=cast(7.0 as float), Price=cast(2 as money), [Sum] = cast(14.0 as money),
		[Series1!TSeries!Array] = null

	-- series for rows
	select [!TSeries!Array]=null, [Id!!Id] = 500, [!TRow.Series1!ParentId] = 78, Price=cast(5 as float)
	union all
	select [!TSeries!Array]=null, [Id!!Id] = 501, [!TRow.Series1!ParentId] = 79, Price=cast(10 as float)

	-- maps for product
	select [!TProduct!Map] = null, [Id!!Id] = 782, [Name!!Name] = N'Product 782',
		[Unit.Id!TUnit!Id] = 7, [Unit.Name!TUnit!Name] = N'Unit7'
	union all
	select [!TProduct!Map] = null, [Id!!Id] = 785, [Name!!Name] = N'Product 785',
		[Unit.Id!TUnit!Id] = 8, [Unit.Name!TUnit!Name] = N'Unit8'

	-- maps for agent
	select [!TAgent!Map] = null, [Id!!Id]=512, [Name!!Name] = 'Agent 512', Code=N'Code 512';
end
go
------------------------------------------------
create or alter procedure a2test.ComplexModelTyped
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;
	declare @date datetime;
	set @date = N'2023-04-20 12:30:17';

	select [Document!TDocument!Object] = null, [Id!!Id] = 123, [No]='DocNo', [Date]= @date,
		[Agent!TAgent!RefId] = 512, [Company!TAgent!RefId] = 512,
		[Rows1!TRow!Array] = null, [Rows2!TRow!Array] = null;

	select [!TRow!Array] = null, [Id!!Id] = 78, [!TDocument.Rows1!ParentId] = 123, 
		[Product!TProduct!RefId] = 782,
		Qty=cast(4.0 as float), Price=cast(8 as money), [Sum] = cast(32.0 as money),
		[Series1!TSeries!Array] = null

	select [!TRow!Array] = null, [Id!!Id] = 79, [!TDocument.Rows2!ParentId] = 123, 
		[Product!TProduct!RefId] = 785,
		Qty=cast(7.0 as float), Price=cast(2 as money), [Sum] = cast(14.0 as money),
		[Series1!TSeries!Array] = null

	-- series for rows
	select [!TSeries!Array]=null, [Id!!Id] = 500, [!TRow.Series1!ParentId] = 78, Price=cast(5 as float)
	union all
	select [!TSeries!Array]=null, [Id!!Id] = 501, [!TRow.Series1!ParentId] = 79, Price=cast(10 as float)

	-- maps for product
	select [Products!TProduct!Map] = null, [Id!!Id] = 782, [Name!!Name] = N'Product 782'
		-- [Unit.Id!TUnit!Id] = 7, [Unit.Name!TUnit!Name] = N'Unit7' - not supported!
	union all
	select [Products!TProduct!Map] = null, [Id!!Id] = 785, [Name!!Name] = N'Product 785'
		-- [Unit.Id!TUnit!Id] = 8, [Unit.Name!TUnit!Name] = N'Unit8'

	-- maps for agent
	select [Agents!TAgent!Map] = null, [Id!!Id]=512, [Name!!Name] = 'Agent 512', Code=N'Code 512';
end
go
------------------------------------------------
create or alter procedure a2test.ComplexModelTypedArray
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;
	select [Documents!TDocument!Array] = null, [Id!!Id] = 123, [No]='DocNo', [Date]=a2sys.fn_getCurrentDate(),
		[Agent!TAgent!RefId] = 512, [Company!TAgent!RefId] = 512,
		[Rows1!TRow!Array] = null, [Rows2!TRow!Array] = null;

	select [!TRow!Array] = null, [Id!!Id] = 78, [!TDocument.Rows1!ParentId] = 123, 
		[Product!TProduct!RefId] = 782,
		Qty=cast(4.0 as float), Price=cast(8 as money), [Sum] = cast(32.0 as money),
		[Series1!TSeries!Array] = null

	select [!TRow!Array] = null, [Id!!Id] = 79, [!TDocument.Rows2!ParentId] = 123, 
		[Product!TProduct!RefId] = 785,
		Qty=cast(7.0 as float), Price=cast(2 as money), [Sum] = cast(14.0 as money),
		[Series1!TSeries!Array] = null

	-- series for rows
	select [!TSeries!Array]=null, [Id!!Id] = 500, [!TRow.Series1!ParentId] = 78, Price=cast(5 as float)
	union all
	select [!TSeries!Array]=null, [Id!!Id] = 501, [!TRow.Series1!ParentId] = 79, Price=cast(10 as float)

	-- maps for product
	select [Products!TProduct!Map] = null, [Id!!Id] = 782, [Name!!Name] = N'Product 782'
		-- [Unit.Id!TUnit!Id] = 7, [Unit.Name!TUnit!Name] = N'Unit7' - not supported!
	union all
	select [Products!TProduct!Map] = null, [Id!!Id] = 785, [Name!!Name] = N'Product 785'
		-- [Unit.Id!TUnit!Id] = 8, [Unit.Name!TUnit!Name] = N'Unit8'

	-- maps for agent
	select [Agents!TAgent!Map] = null, [Id!!Id]=512, [Name!!Name] = 'Agent 512', Code=N'Code 512';
end
go
------------------------------------------------
create or alter procedure a2test.TreeModel
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;

	select [Menu!TMenu!Tree]=null, [!!Id] = 10, [!TMenu.Menu!ParentId]=null, [Name!!Name]=N'Item 1',
		[Menu!TMenu!Items] = null
	union all
	select [Menu!TMenu!Tree]=null, [!!Id] = 20, [!TMenu.Menu!ParentId]=null, [Name!!Name]=N'Item 2',
		[Menu!TMenu!Items] = null
	union all
	select [Menu!TMenu!Tree]=null, [!!Id] = 110, [!TMenu.Menu!ParentId]=10, [Name!!Name]=N'Item 1.1',
		[Menu!TMenu!Items] = null
	union all
	select [Menu!TMenu!Tree]=null, [!!Id] = 120, [!TMenu.Menu!ParentId]=10, [Name!!Name]=N'Item 1.2',
		[Menu!TMenu!Items] = null
	union all
	select [Menu!TMenu!Tree]=null, [!!Id] = 1100, [!TMenu.Menu!ParentId]=110, [Name!!Name]=N'Item 1.1.1',
		[Menu!TMenu!Items] = null
end
go
------------------------------------------------
create or alter procedure a2test.EmptyTreeModel
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;

	select [Menu!TMenu!Tree]=null, [!!Id] = 10, [!TMenu.Menu!ParentId]=null, [Name!!Name]=N'Item 1',
		[Menu!TMenu!Items] = null
	where 0 <> 0
end
go
------------------------------------------------
create or alter procedure a2test.GroupModel
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;

	declare @Table table(Company nvarchar(255), Agent nvarchar(255), Amount money);
	insert into @Table (Company, Agent, Amount)
	values
		(N'Company 1', N'Agent 1', 400),
		(N'Company 1', N'Agent 2', 100),
		(N'Company 2', N'Agent 1', 40),
		(N'Company 2', N'Agent 2', 10);

	select [Model!TModel!Group] = null, 
		Company,
		Agent,
		Amount = sum(Amount),
		[Company!!GroupMarker] = grouping(Company),
		[Agent!!GroupMarker] = grouping(Agent),
		[Items!TModel!Items]=null	 -- array for nested elements
	from @Table
	group by rollup(Company, Agent)
	order by grouping(Company) desc, grouping(Agent) desc, Company, Agent;
end
go
------------------------------------------------
create or alter procedure a2test.MapRoot
@UserId bigint = 0
as
begin
	set nocount on;
	select [Model!TModel!Map] = null, [Key!!Key] = N'Key1', [Id!!Id] = 11, [Name!!Name]='Object 1'
	union all
	select [Model!TModel!Map] = null, [Key!!Key] = N'Key2', [Id!!Id] = 12, [Name!!Name]='Object 2';
end
go
------------------------------------------------
create or alter procedure a2test.EmptyArray
	@TenantId int = null,
	@UserId bigint = 0
as
begin
	set nocount on;
	select [Model!TModel!Object] = null, [Key!!Id] = N'Key1', [ModelName!!Name]='Object 1',
		[Rows!TRow!Array] = null
end
go

/*
------------------------------------------------
-- test subobjects (update)
{
	Id: 45,
	Name: 'MainObjectName',
	NumValue : 531.55,
	BitValue : true,
	SubObject : {
		Id: 55,
		Name: 'SubObjectName',
		SubArray: [
			{X: 5, Y:6, D:5.1 },
			{X: 8, Y:9, D:7.23 }
		]
	}
}
------------------------------------------------
*/
drop procedure if exists a2test.[NestedObject.Metadata];
drop procedure if exists a2test.[NestedObject.Update];
drop procedure if exists a2test.[NewObject.Metadata];
drop procedure if exists a2test.[NewObject.Update];
drop procedure if exists a2test.[SubObjects.Metadata];
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'SubObjects.Update')
	drop procedure a2test.[SubObjects.Update]
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'Json.Metadata')
	drop procedure a2test.[Json.Metadata]
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'Json.Update')
	drop procedure a2test.[Json.Update]
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'ParentKey.Metadata')
	drop procedure a2test.[ParentKey.Metadata]
go
------------------------------------------------
drop procedure if exists a2test.[Document.RowsMethods.Metadata];
drop procedure if exists a2test.[Document.RowsMethodsTyped.Metadata];
drop procedure if exists a2test.[Document.RowsMethods.Update];
drop procedure if exists a2test.[Document.RowsMethodsTyped.Update];
drop procedure if exists a2test.[Guid.Metadata];
drop procedure if exists a2test.[Guid.Update];
drop procedure if exists a2test.[Nullable.SaveModel.Metadata];
drop procedure if exists a2test.[Nullable.SaveModel.Update];
drop procedure if exists a2test.[Fallback.Metadata];
drop procedure if exists a2test.[Fallback.Update];
drop procedure if exists a2test.[EmptyString.Metadata];
drop procedure if exists a2test.[EmptyString.Update];
drop procedure if exists a2test.[Agent.SameProps.Metadata];
drop procedure if exists a2test.[Agent.SameProps.Update];
drop procedure if exists a2test.[ScalarTypes.Metadata]
drop procedure if exists a2test.[ScalarTypes.Update]
go
------------------------------------------------
drop type if exists [a2test].[NestedMain.TableType];
drop type if exists [a2test].[NestedSub.TableType];
drop type if exists [a2test].[NestedSubArray.TableType];
drop type if exists [a2test].[Document.TableType];
drop type if exists [a2test].[Row.TableType];
drop type if exists [a2test].[Method.TableType];
drop type if exists [a2test].[MethodData.TableType];
drop type if exists [a2test].[GuidMain.TableType];
drop type if exists [a2test].[GuidRow.TableType];
drop type if exists [a2test].[Id.TableType];
drop type if exists [a2test].[Tag.TableType];
drop type if exists [a2test].[ScalarTypes.TableType];
go
------------------------------------------------
create type [a2test].[NestedSub.TableType] as
table (
	[Id] bigint null,
	[ParentId] bigint null,
	[ParentKey] nvarchar(255) null,
	[Name] nvarchar(255),
	GuidValue uniqueidentifier,
	ParentGUID uniqueidentifier
)
go
------------------------------------------------
create type [a2test].[NestedSubArray.TableType] as
table (
	[Id] bigint null,
	[ParentId] bigint null,
	[X] int,
	[Y] int,
	[D] decimal(10, 5)
)
go
------------------------------------------------
create type [a2test].[Document.TableType] as
table (
	[Id] bigint null,
	[Name] nvarchar(255)
)
go
------------------------------------------------
create type [a2test].[Row.TableType] as
table (
	[Id] bigint null,
	[RowNumber] int null,
	[Nested.Key] int null,
	[Nested.Value] nvarchar(255) null
)
go
------------------------------------------------
create type [a2test].[Method.TableType] as
table (
	[Id] bigint null,
	[CurrentKey] nvarchar(255) null,
	ParentRowNumber int null,
	[Name] nvarchar(255) null
)
go
------------------------------------------------
create type [a2test].[MethodData.TableType] as
table (
	[Id] int null,
	ParentRowNumber int null,
	ParentKey nvarchar(255) null,
	[Code] nvarchar(255) null
)
go
------------------------------------------------
create type a2test.[NestedMain.TableType] as
table (
	[Id] bigint null,
	[Name] nvarchar(255),
	[NumValue] float,
	[BitValue] bit,
	[GuidValue] uniqueidentifier,
	[SubObject] bigint,
	[SubObjectString] nchar(4),
	[GUID] uniqueidentifier
)
go
------------------------------------------------
create type [a2test].[GuidMain.TableType] as
table (
	[Id] bigint null,
	[GUID] uniqueidentifier
)
go
------------------------------------------------
create type [a2test].[GuidRow.TableType] as
table (
	[Id] bigint,
	[GUID] uniqueidentifier,
	ParentId bigint, 
	[ParentGUID] uniqueidentifier,
	RowNumber int,
	ParentRowNumber int,
	[Code] nvarchar(255)
)
go
------------------------------------------------
create type [a2test].[Id.TableType] as
table (
	[Id] bigint null,
	[RowNumber] int null
)
go
------------------------------------------------
create type [a2test].[Tag.TableType] as
table (
	--[GUID] uniqueidentifier,
	--[ParentGUID] uniqueidentifier,
	[Id] bigint null,
	[RowNumber] int null,
	[ParentId] bigint null
);
go
------------------------------------------------
create type [a2test].[ScalarTypes.TableType] as 
table (
	Int32Value int,
	Int64Value bigint,
	StringValue nvarchar(255),
	MoneyValue money,
	FloatValue float,
	BitValue bit,
	GuidValue uniqueidentifier,
	DateTimeValue datetime
);
go
------------------------------------------------
create procedure a2test.[NestedObject.Metadata]
as
begin
	set nocount on;
	declare @NestedMain [a2test].[NestedMain.TableType];
	declare @SubObject [a2test].[NestedSub.TableType];
	declare @SubObjectArray [a2test].[NestedSubArray.TableType];
	select [MainObject!MainObject!Metadata]=null, * from @NestedMain;
	select [SubObject!MainObject.SubObject!Metadata]=null, * from @SubObject;
	select [SubObjectArray!MainObject.SubObject.SubArray!Metadata]=null, * from @SubObjectArray;
end
go
------------------------------------------------
create procedure a2test.[NewObject.Metadata]
as
begin
	set nocount on;
	declare @NestedMain [a2test].[NestedMain.TableType];
	select [MainObject!MainObject!Metadata]=null, * from @NestedMain;
end
go
------------------------------------------------
create procedure a2test.[NestedObject.Update]
	@TenantId int = null,
	@UserId bigint = null,
	@MainObject [a2test].[NestedMain.TableType] readonly,
	@SubObject [a2test].[NestedSub.TableType] readonly,
	@SubObjectArray [a2test].[NestedSubArray.TableType] readonly
as
begin
	set nocount on;
	
	--declare @msg nvarchar(max);
	--set @msg = (select * from @SubObjectArray for xml auto);
	-- raiserror(@msg, 16, -1) with nowait;

	select [MainObject!TMainObject!Object] = null, [Id!!Id] = Id, [Name!!Name] = [Name],
		NumValue, BitValue, GuidValue,
		[SubObject!TSubObject!RefId] = SubObject, [GUID]
	from @MainObject;

	select [!TSubObject!Map] = null, [Id!!Id] = Id, [Name!!Name] = Name, [!TMainObject.SubObject!ParentId] = ParentId,
		[SubArray!TSubObjectArrayItem!Array] = null,
		ParentGuid = ParentGUID
	from @SubObject;

	select [!TSubObjectArrayItem!Array] = null, [X] = X, [Y] = Y, [D] = D, [!TSubObject.SubArray!ParentId] = ParentId
	from @SubObjectArray;
end
go
------------------------------------------------
create procedure a2test.[NewObject.Update]
	@TenantId int = null,
	@UserId bigint = null,
	@MainObject [a2test].[NestedMain.TableType] readonly
as
begin
	set nocount on;
	
	if exists(select * from @MainObject where Id is null)
		select [MainObject!TMainObject!Object] = null, [Name] = N'Id is null';
	else
		select [MainObject!TMainObject!Object] = null, [Name] = N'Id is not null';
end
go
------------------------------------------------
create procedure a2test.[SubObjects.Metadata]
as
begin
	set nocount on;
	declare @NestedMain [a2test].[NestedMain.TableType];
	select [MainObject!MainObject!Metadata]=null, * from @NestedMain;
end
go
------------------------------------------------
create procedure a2test.[SubObjects.Update]
	@TenantId int = null,
	@UserId bigint = null,
	@MainObject [a2test].[NestedMain.TableType] readonly
as
begin
	set nocount on;
	
	declare @rootId nvarchar(255) = N'not null';
	declare @subId nvarchar(255) = N'not null';
	declare @subIdString nvarchar(255) = N'not null';
	if (select Id from @MainObject) is null
		set @rootId  = N'null';
	if (select SubObject from @MainObject) is null
		set @subId = N'null'
	if (select SubObjectString from @MainObject) is null
		set @subIdString = N'null'
	select [MainObject!TMainObject!Object] = null, [RootId] = @rootId, [SubId] = @subId, SubIdString = @subIdString;
end
go
------------------------------------------------
create procedure a2test.[Json.Metadata]
as
begin
	set nocount on;
	declare @NestedMain [a2test].[NestedMain.TableType];
	select [MainObject!MainObject!Metadata]=null, * from @NestedMain;
	select [JsonData!MainObject.SubObject!Json]=null;
end
go
------------------------------------------------
create procedure a2test.[Json.Update]
	@TenantId int = null,
	@UserId bigint = null,
	@JsonData nvarchar(max) = null,
	@MainObject [a2test].[NestedMain.TableType] readonly
as
begin
	set nocount on;
	
	--throw 60000, @JsonData, 0;

	declare @rootId nvarchar(255) = N'not null';
	declare @subId nvarchar(255) = N'not null';
	declare @subIdString nvarchar(255) = N'not null';
	if (select Id from @MainObject) is null
		set @rootId  = N'null';
	if (select SubObject from @MainObject) is null
		set @subId = N'null'
	if (select SubObjectString from @MainObject) is null
		set @subIdString = N'null'

	select [MainObject!TMainObject!Object] = null, [RootId] = @rootId, [SubId] = @subId, SubIdString = @subIdString,
		[SubJson!!Json] = @JsonData;

	select [RootJson!!Json] = @JsonData;
end
go
------------------------------------------------
create procedure a2test.[ParentKey.Metadata]
as
begin
	set nocount on;
	declare @NestedMain [a2test].[NestedMain.TableType];
	declare @SubObjects [a2test].[NestedSub.TableType];
	select [MainObject!MainObject!Metadata]=null, * from @NestedMain;
	select [SubObjects!MainObject.SubObjects!Metadata] = null, * from @SubObjects;
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'ComplexObjects')
	drop procedure a2test.ComplexObjects
go
------------------------------------------------
create procedure a2test.ComplexObjects
@UserId bigint = null
as
begin
	set nocount on;
	select [Document!TDocument!Object] = null, [Id!!Id]=200, [Agent.Id!TAgent!Id] = 300, [Agent.Name!TAgent] = 'Agent name';
end
go


------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'RefObjects')
	drop procedure a2test.RefObjects
go
------------------------------------------------
create procedure a2test.RefObjects
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;
	select [Document!TDocument!Object] = null, [Id!!Id]=200, [Agent!TAgent!RefId] = 300, [Company!TAgent!RefId]= 500;

	select [!TAgent!Map] = null, [Id!!Id] = 300, Name = N'Agent Name'
	union all
	select [!TAgent!Map] = null, [Id!!Id] = 500, Name = N'Company Name';
end
go
------------------------------------------------
create or alter procedure a2test.[Document.Load]
	@TenantId int = null,
	@UserId bigint = null,
	@Id bigint = null,
	@Date datetime = null
as
begin
	set nocount on;
	select [Document!TDocument!Object] = null, [Id!!Id]=@Id, [Agent!TAgent!RefId] = 300, [Date] = @Date,
	[Company!TAgent!RefId]= 500, 
	[PriceList!TPriceList!RefId] = 1,
	[PriceKind!TPriceKind!RefId] = 7,
	[Rows!TRow!Array] = null

	select [!TRow!Array] = null, [Id!!Id] = 59, [!TDocument.Rows!ParentId] = @Id,
		[PriceKind!TPriceKind!RefId] = 7, [Entity!TEntity!RefId] = 96;

	select [!TAgent!Map] = null, [Id!!Id] = 300, Name = N'Agent Name'
	union all
	select [!TAgent!Map] = null, [Id!!Id] = 500, Name = N'Company Name';

	select [!TEntity!Map] = null, [Id!!Id] = 96, Name = N'Entity Name',
		[Prices!TPrice!Array] = null;

	select [PriceLists!TPriceList!Array] = null, [Id!!Id] = 1, 
		Name = N'PriceList', [PriceKinds!TPriceKind!Array] = null;

	select [PriceKinds!TPriceKind!Array] = null, [Id!!Id] = 7,
		[!TPriceList.PriceKinds!ParentId] = 1, Name=N'Kind', 
		[Prices!TPrice!Array] = null
	union all
	select [PriceKinds!TPriceKind!Array] = null, [Id!!Id] = 8,
		[!TPriceList.PriceKinds!ParentId] = 1, Name=N'Kind', 
		[Prices!TPrice!Array] = null;

	select [!TPrice!Array] = null, [Id!!Id] = 40, [!TPriceKind.Prices!ParentId] = 7, 
		[!TEntity.Prices!ParentId] = 96, [PriceKind!TPriceKind!RefId] = 8,
		Price = 22.5
	union all
	select [!TPrice!Array] = null, [Id!!Id] = 41, [!TPriceKind!Prices.ParentId] = 7, 
		[!TEntity.Prices!ParentId] = 96, [PriceKind!TPriceKind!RefId] = 8,
		Price = 36.8
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'Document2.Load')
	drop procedure a2test.[Document2.Load]
go
------------------------------------------------
create procedure a2test.[Document2.Load]
	@TenantId int = null,
	@UserId bigint = null,
	@Id bigint = null
as
begin
	set nocount on;
	select [Document!TDocument!Object] = null, [Id!!Id]=@Id,
	[Rows!TRow!Array] = null, [PriceKind!TPriceKind!RefId] = cast(4294967306 as bigint)

	select [!TRow!Array] = null, [Id!!Id] = 59, [!TDocument.Rows!ParentId] = @Id,
		[Entity!TEntity!RefId] = cast(4295140867 as bigint);

	select [!TEntity!Map] = null, [Id!!Id] = cast(4295140867 as bigint), Name = N'Entity Name',
		[Prices!TPrice!Array] = null;

	select [PriceLists!TPriceList!Array] = null, [Id!!Id] = cast(4294967300 as bigint), 
		Name = N'PriceList', [PriceKinds!TPriceKind!Array] = null
	union all
	select [PriceLists!TPriceList!Array] = null, [Id!!Id] = cast(4294967304 as bigint), 
		Name = N'PriceList', [PriceKinds!TPriceKind!Array] = null;

	select [PriceKinds!TPriceKind!Array] = null, [Id!!Id] = cast(4294967305  as bigint), [Name!!Name]=N'Kind 1', Main=1, [!TPriceList.PriceKinds!ParentId] = cast(4294967304 as bigint) 
	union all
	select [PriceKinds!TPriceKind!Array] = null, [Id!!Id] = cast(4294967304 as bigint), [Name!!Name]=N'Kind 2', Main=0, [!TPriceList.PriceKinds!ParentId] = cast(4294967304 as bigint)
	union all
	select [PriceKinds!TPriceKind!Array] = null, [Id!!Id] = cast(4294967306 as bigint), [Name!!Name]=N'Kind 3', Main=0, [!TPriceList.PriceKinds!ParentId] = cast(4294967304 as bigint)
	union all
	select [PriceKinds!TPriceKind!Array] = null, [Id!!Id] = cast(4294967303 as bigint), [Name!!Name]=N'Kind 4', Main=0, [!TPriceList.PriceKinds!ParentId] = cast(4294967304 as bigint)

	select [!TPrice!Array] = null, [PriceKind!TPriceKind!RefId] = cast(4294967305 as bigint),
		[!TEntity.Prices!ParentId] = cast(4295140867 as bigint), Price = 185.7
	union all
	select [!TPrice!Array] = null, [PriceKind!TPriceKind!RefId] = cast(4294967304 as bigint),
		[!TEntity.Prices!ParentId] = cast(4295140867 as bigint), Price = 179.4
	union all
	select [!TPrice!Array] = null, [PriceKind!TPriceKind!RefId] = cast(4294967306 as bigint),
		[!TEntity.Prices!ParentId] = cast(4295140867 as bigint), Price = 172.44
end
go

------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'Document.Aliases')
	drop procedure a2test.[Document.Aliases]
go
------------------------------------------------
create procedure a2test.[Document.Aliases]
	@TenantId int = null,
	@UserId bigint = null,
	@Id bigint = null
as
begin
	set nocount on;
	select [!$Aliases!] = null, [~Document] = N'Document!TDocument!Object', [~TRow] = N'!TRow!Array', [~EntRef] = N'Entity!TEntity!RefId'

	select [~Document] = null, [Id!!Id]=@Id,
	[Rows!TRow!Array] = null

	select [~TRow] = null, [Id!!Id] = 59, [!TDocument.Rows!ParentId] = @Id,
		[~EntRef] = cast(59 as bigint);

	select [!TEntity!Map] = null, [Id!!Id] = cast(276 as bigint), Name = N'Entity Name'
end
go

------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'SimpleModel.Localization.Load')
	drop procedure a2test.[SimpleModel.Localization.Load]
go
------------------------------------------------
create procedure a2test.[SimpleModel.Localization.Load]
	@TenantId int = null,
	@UserId bigint = null,
	@Id bigint = null
as
begin
	set nocount on;
	select [Model!TModel!Object] = null, [Id!!Id] = 234, [Name!!Name]='@[Item1]';
end
go

------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'ComplexObject.Localization.Load')
	drop procedure a2test.[ComplexObject.Localization.Load]
go
------------------------------------------------
create procedure a2test.[ComplexObject.Localization.Load]
@UserId bigint = null
as
begin
	set nocount on;
	select [Document!TDocument!Object] = null, [Id!!Id]=200, [Agent.Id!TAgent!Id] = 300, [Agent.Name!TAgent] = '@[Item2]';
end
go


------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'TypesModel.Load')
	drop procedure a2test.[TypesModel.Load]
go
------------------------------------------------
create procedure a2test.[TypesModel.Load]
	@TenantId int = null,
	@UserId bigint = null,
	@Id bigint = null
as
begin
	set nocount on;
	select [Model!TModel!Object] = null, [Id!!Id] = 123, 
		[Name!!Name]='ObjectName', [Decimal] = cast(55.1234 as decimal(10, 5)),
		[Int] = cast(32 as int),
		[BigInt] = cast(77223344 as bigint),
		[Short] = cast(27823 as smallint),
		[Tiny] = cast(255 as tinyint),
		[Float] = cast(77.6633 as float),
		[Date] = cast(N'20180219' as date),
		[DateTime] = cast(N'20180219 15:10:20' as datetime)
end
go
------------------------------------------------
create or alter procedure a2test.[TypesModel.Params]
	@TenantId int = null,
	@UserId bigint = null,
	@Date date,
	@Date2 datetime
as
begin
	set nocount on;
	select [Date] = @Date, [Date2] = @Date2;
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'PagerModel.Load')
	drop procedure a2test.[PagerModel.Load]
go
------------------------------------------------
create procedure a2test.[PagerModel.Load]
	@TenantId int = null,
	@UserId bigint = null,
	@Id bigint = null
as
begin
	set nocount on;
	select [Elems!TElem!Array] = null, [Id!!Id] = 1, [Name!!Name]=N'ItemName',
	[!!RowCount] = 27;

	select [!$System!] = null, [!Elems!PageSize] = 20, [!Elems!SortOrder] = N'Name', [!Elems!SortDir] = 'asc';
end
go

------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'EmptyArray2')
	drop procedure a2test.EmptyArray2
go
------------------------------------------------
create procedure a2test.EmptyArray2
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;
	select [Elements!TElem!Array] = null, [Id!!Id] = 11, [Name!!Name]='Elem Name'
	where 0 <> 0

	select [!$System!] = null, [!Elements!PageSize] = 20, [!Elements!SortOrder] = N'Name', [!Elements!SortDir] = 'asc';
end
go

------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'SubObjects.Load')
	drop procedure a2test.[SubObjects.Load]
go
------------------------------------------------
create procedure a2test.[SubObjects.Load]
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;

	select [Document!TDocument!Object] = null, [Id!!Id] = 234, [Name!!Name]=N'Document name', 
		[Contract!TContract!Object] = null;

	select [!TContract!Object] = null, [Id!!Id] = 421, [Name!!Name]=N'Contract name', [!TDocument.Contract!ParentId] = 234;
end
go

------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'MapObjects.Load')
	drop procedure a2test.[MapObjects.Load]
go
------------------------------------------------
create procedure a2test.[MapObjects.Load]
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;

	select [Document!TDocument!Object] = null, [Id!!Id] = 234, [Name!!Name]=N'Document name', [Category!TCategory!RefId] = N'CAT1';


	select [Categories!TCategory!Map] = null, [Id!!Id] = N'CAT1', [Key!!Key] = N'CAT1', [Name!!Name]=N'Category_1';
end
go

------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'MapObjects.NoKey.Load')
	drop procedure a2test.[MapObjects.NoKey.Load]
go
------------------------------------------------
create procedure a2test.[MapObjects.NoKey.Load]
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;

	select [Document!TDocument!Object] = null, [Id!!Id] = 234, [Name!!Name]=N'Document name', [Category!TCategory!RefId] = N'CAT1';

	-- no keys. will be represened as an array
	select [Categories!TCategory!Map] = null, [Id!!Id] = N'CAT1'/*, [Key!!Key] = N'CAT1'*/, [Name!!Name]=N'Category_1';
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'Document.RowsMethods.Load')
	drop procedure a2test.[Document.RowsMethods.Load]
go
------------------------------------------------
create procedure a2test.[Document.RowsMethods.Load]
	@TenantId int = null,
	@UserId bigint = null,
	@Id bigint = null
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	select [Document!TDocument!Object] = null, [Id!!Id] = 123, [Name!!Name]='Document', [Rows!TRow!Array] = null;

	select [!TRow!Array] = null, [Id!!Id]= 7, [!TDocument.Rows!ParentId]=123, [Methods!TMethod!MapObject!Mtd1:Mtd2] = null;

	select [!TMethod!MapObject] = null, [Id!!Id] = 11, [Name!!Name] = N'Method 1', [!TRow.Methods!ParentId] = 7, [Key!!Key] = N'Mtd1',
		[Data!TMethodData!Array] = null
	union all
	select [!TMethod!MapObject] = null, [Id!!Id] = 22, [Name!!Name] = N'Method 2', [!TRow.Methods!ParentId] = 7, [Key!!Key] = N'Mtd2',
		[Data!TMethodData!Array] = null

	select [!TMethodData!Array] = null, [Id!!Id] = 276, Code='Code1', [!TMethod.Data!ParentId] = 11;
end
go
------------------------------------------------
create procedure a2test.[Document.RowsMethods.Metadata]
as
begin
	set nocount on;
	declare @Document [a2test].[Document.TableType];
	declare @Rows [a2test].[Row.TableType];
	declare @Methods [a2test].[Method.TableType];
	declare @MethodData [a2test].[MethodData.TableType];
	select [Document!Document!Metadata]=null, * from @Document;
	select [Rows!Document.Rows!Metadata]=null, * from @Rows;
	select [Methods!Document.Rows.Methods*!Metadata]=null, * from @Methods;
	select [MethodData!Document.Rows.Methods*.Data!Metadata]=null, * from @MethodData;
end
go
------------------------------------------------
create procedure a2test.[Document.RowsMethodsTyped.Metadata]
as
begin
	set nocount on;
	declare @Document [a2test].[Document.TableType];
	declare @Rows [a2test].[Row.TableType];
	declare @Methods [a2test].[Method.TableType];
	declare @MethodData [a2test].[MethodData.TableType];
	select [Document!Document!Metadata]=null, * from @Document;
	select [Rows!Document.Rows!Metadata]=null, * from @Rows;
	select [Methods!Document.Rows.Methods*!Metadata]=null, * from @Methods;
	select [MethodData!Document.Rows.Methods*.Data!Metadata]=null, * from @MethodData;
end
go
------------------------------------------------
create procedure a2test.[Document.RowsMethods.Update]
	@TenantId int = null,
	@UserId bigint = null,
	@Document [a2test].[Document.TableType] readonly,
	@Rows [a2test].[Row.TableType] readonly,
	@Methods [a2test].[Method.TableType] readonly,
	@MethodData [a2test].[MethodData.TableType] readonly
as
begin
	set nocount on;
	select [Document!TDocument!Object] = null, [Id!!Id] = Id, [Name!!Name] = [Name]
	from @Document;
	
	select [Rows!TRow!Array] = null, [Id!!Id] = Id, RowNo=RowNumber
	from @Rows;

	select [Methods!TMethod!Array] = null, [Key]=CurrentKey, [Name], RowNo=ParentRowNumber
	from @Methods;

	select [MethodData!TMethodData!Array] = null, [Code], RowNo=ParentRowNumber, [Key]=ParentKey
	from @MethodData;
end
go
------------------------------------------------
create procedure a2test.[Document.RowsMethodsTyped.Update]
	@TenantId int = null,
	@UserId bigint = null,
	@Document [a2test].[Document.TableType] readonly,
	@Rows [a2test].[Row.TableType] readonly,
	@Methods [a2test].[Method.TableType] readonly,
	@MethodData [a2test].[MethodData.TableType] readonly
as
begin
	set nocount on;

	select [Document!TDocument!Object] = null, [Id!!Id] = Id, [Name!!Name]=[Name], [Rows!TRow!Array] = null
	from @Document

	select [!TRow!Array] = null, [Id!!Id]= Id, [!TDocument.Rows!ParentId]=cast(123 as bigint), [Methods!TMethod!MapObject!Mtd1:Mtd2] = null
	from @Rows

	select [!TMethod!MapObject] = null, [Id!!Id] = Id, [Name!!Name] = [Name], [!TRow.Methods!ParentId] = cast(7 as bigint), [Key!!Key] = [CurrentKey],
		[Data!TMethodData!Array] = null
	from @Methods

	select [!TMethodData!Array] = null, [Id!!Id] = Id, Code, [!TMethod.Data!ParentId] = cast(11 as bigint)
	from @MethodData;
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'InvalidType.Load')
	drop procedure a2test.[InvalidType.Load]
go
------------------------------------------------
create procedure a2test.[InvalidType.Load]
	@TenantId int = null,
	@UserId bigint = null,
	@Id bigint = null
as
begin
	set nocount on;
	select [Model!TModel!Aray] = null, [Id!!Id] = 123;
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'MultipleTypes.Load')
	drop procedure a2test.[MultipleTypes.Load]
go
------------------------------------------------
create procedure a2test.[MultipleTypes.Load]
	@TenantId int = null,
	@UserId bigint = null,
	@Id bigint = null
as
begin
	set nocount on;
	select [Model!TModel!Object] = null, [Id!!Id] = 123, [Agent1!TAgent!RefId] = 5, [Agent2!TAgent!RefId] = 7;
	select [!TAgent!Map] = null, [Id!!Id] = 5, [Name] = N'Five';
	select [!TAgent!Map] = null, [Id!!Id] = 7, [Name] = N'Seven', Memo=N'Memo for seven';
end
go
------------------------------------------------
create procedure a2test.[Guid.Metadata]
as
begin
	set nocount on;
	declare @Document [a2test].[GuidMain.TableType];
	declare @Rows [a2test].[GuidRow.TableType];

	select [Document!Document!Metadata]=null, * from @Document;
	select [Rows!Document.Rows!Metadata]=null, * from @Rows;
	select [SubRows!Document.Rows.SubRows!Metadata]=null, * from @Rows;
end
go
------------------------------------------------
create or alter procedure a2test.[Guid.Update]
@Document [a2test].[GuidMain.TableType] readonly,
@Rows [a2test].[GuidRow.TableType] readonly,
@SubRows [a2test].[GuidRow.TableType] readonly
as
begin
	set nocount on;
	declare @Id bigint = null;
	declare @Guid uniqueidentifier = null;

	select @Id = Id, @Guid = [GUID] from @Document;

	select [Document!TDocument!Object] = null, [Id!!Id] = Id, [Rows!TRow!Array] = null,  
		[GUID] from @Document;

	select [!TRow!Array] = null, [!TDocument.Rows!ParentId]=@Id, 
		[Id!!Id] = Id, Code, ParentGuid = ParentGUID, RowNo = RowNumber, ParentRN = ParentRowNumber, [GUID],
		[SubRows!TSubRow!Array] = null
	from @Rows
	order by Id;

	select [!TSubRow!Array] = null, [!TRow.SubRows!ParentId] = cast(10 as bigint), [GUID],
		Id, Code, ParentGuid=ParentGUID, RowNo = RowNumber, ParentRN = ParentRowNumber
	from @SubRows
	order by Id;
end
go

------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'CrossModel.Load')
	drop procedure a2test.[CrossModel.Load]
go
------------------------------------------------
create procedure a2test.[CrossModel.Load]
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;
	select [RepData!TData!Array] = null, [Id!!Id] = 10, [S1]=N'S1', N1 = 100, [Cross1!TCross!CrossArray] = null
	union all
	select null, 20, N'S2', 200, null;

	select [!TCross!CrossArray] = null, [Key!!Key] = N'K1', Val = 11, [!TData.Cross1!ParentId] = 10
	union all
	select null, N'K2', 22, 20;
end
go
------------------------------------------------
create or alter procedure a2test.[CrossModel_Id.Load]
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;
	select [RepData!TData!Array] = null, [!!Id] = 10, [S1]=N'S1', N1 = 100, [Cross1!TCross!CrossArray] = null
	union all
	select null, 20, N'S2', 200, null;

	select [!TCross!CrossArray] = null, [Key!!Key] = N'K1', Val = 11, [!TData.Cross1!ParentId] = 10
	union all
	select null, N'K2', 22, 20;
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'CrossModelMulti.Load')
	drop procedure a2test.[CrossModelMulti.Load]
go
------------------------------------------------
create procedure a2test.[CrossModelMulti.Load]
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;
	select [RepData!TData!Group] = null, [Id!!Id] = cast(null as int), [Sum] = 100, [Id!!GroupMarker] = cast(1 as bit),
		[CrossDt!TCross!CrossArray] = null, [CrossCt!TCross!CrossArray] = null,
		[Items!TData!Items] = null
	union all
	select null, 10, 100, 0, null, null, null
	union all
	select null, 20, 200, 0, null, null, null
	union all
	select null, 30, 300, 0, null, null, null

	-- cross dt
	select [!TCross!CrossArray] = null, [Acc!!Key] = N'A1', [Sum] = 11, [!TData.CrossDt!ParentId] = 10
	union all
	select null, N'A2', 22, 10
	union all
	select null, N'A2', 33, 20
	order by 2;

	-- cross ct
	select [!TCross!CrossArray] = null, [Acc!!Key] = N'A3', [Sum] = 33, [!TData.CrossCt!ParentId] = 10
	union all
	select null, N'A2', 44, 10
	order by 2;
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'CrossModelObj.Load')
	drop procedure a2test.[CrossModelObj.Load]
go
------------------------------------------------
create procedure a2test.[CrossModelObj.Load]
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;
	select [RepData!TData!Array] = null, [Id!!Id] = 10, [S1]=N'S1', N1 = 100, [Cross1!TCross!CrossObject] = null
	union all
	select null, 20, N'S2', 200, null;

	select [!TCross!CrossObject] = null, [Key!!Key] = N'K1', Val = 11, [!TData.Cross1!ParentId] = 10
	union all
	select null, N'K2', 22, 20;
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'GroupWithCross.Load')
	drop procedure a2test.[GroupWithCross.Load]
go
------------------------------------------------
create procedure a2test.[GroupWithCross.Load]
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;

	declare @Table table(Company nvarchar(255), Agent nvarchar(255), Amount money, Id int);
	insert into @Table (Company, Agent, Amount, Id)
	values
		(N'Company1', N'Agent1', 400, 10),
		(N'Company1', N'Agent2', 100, 11),
		(N'Company2', N'Agent1', 40, 12),
		(N'Company2', N'Agent2', 10, 13);

	select [Model!TModel!Group] = null, 
		[Id!!Id] = N'[' + Company + N':' + Agent + N']',
		Company,
		Agent,
		Amount = sum(Amount),
		[Company!!GroupMarker] = grouping(Company),
		[Agent!!GroupMarker] = grouping(Agent),
		[Items!TModel!Items]=null,	 -- array for nested elements
		[Cross1!TCross!CrossArray] = null
	from @Table
	group by rollup(Company, Agent)
	order by grouping(Company) desc, grouping(Agent) desc, Company, Agent;

	select [!TCross!CrossArray] = null, [Key!!Key] = N'K1', Val = 11, [!TModel.Cross1!ParentId] = N'[Company1:Agent1]'
	union all
	select null, N'K2', 22, N'[Company2:Agent2]';
end
go

------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'UtcDate.Load')
	drop procedure a2test.[UtcDate.Load]
go
------------------------------------------------
create procedure a2test.[UtcDate.Load]
	@TenantId int = null,
	@UserId bigint = null,
	@Id bigint = null
as
begin
	set nocount on;
	select [Model!TModel!Object] = null, [Date] = getdate(), [UtcDate!!Utc] = getutcdate();
end
go
------------------------------------------------
create or alter procedure a2test.[BatchModel.Load]
	@TenantId int = null,
	@UserId bigint = null,
	@Id bigint = null
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	select [Document!TDocument!Object] = null, [Id!!Id] = Id, [Name!!Name]=[Name], Memo,
		[Rows!TRow!Array] = null
	from a2test.Documents where Id=@Id;

	select [!TRow!Array] = null, [Id!!Id] = Id, Memo, Qty, [!TDocument.Rows!ParentId] = Document
	from a2test.[Rows] where Document = @Id;
end
go
------------------------------------------------
drop procedure if exists a2test.[BatchModel.Metadata];
drop procedure if exists a2test.[BatchModel.Update];
drop type if exists [a2test].[DocumentBatch.TableType];
drop type if exists [a2test].[RowBatch.TableType];
go
------------------------------------------------
create type [a2test].[DocumentBatch.TableType] as
table (
	[Id] bigint null,
	[GUID] uniqueidentifier,
	[Name] nvarchar(255),
	[Date] datetime,
	[Sum] money,
	[Memo] nvarchar(255)
)
go
------------------------------------------------
create type [a2test].[RowBatch.TableType] as
table (
	[Id] bigint null,
	[ParentGUID] uniqueidentifier,
	[Memo] nvarchar(255),
	Qty float,
	Price money,
	[Sum] money
)
go
------------------------------------------------
create or alter procedure a2test.[BatchModel.Metadata]
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;
	declare @Document [a2test].[DocumentBatch.TableType];
	declare @Rows [a2test].[RowBatch.TableType];

	select [Document!Document!Metadata] = null, * from @Document;
	select [Rows!Document.Rows!Metadata] = null, * from @Rows;
end
go
------------------------------------------------
create or alter procedure a2test.[BatchModel.Update]
@TenantId int = null,
@UserId bigint = null,
@Document [a2test].[DocumentBatch.TableType] readonly,
@Rows [RowBatch.TableType] readonly
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;
	set xact_abort on;

	declare @rtable table(Id bigint);
	declare @Id bigint;

	begin tran;
	merge a2test.Documents as t
	using @Document as s
	on t.Id = s.Id
	when matched then update set
		t.[Date] = s.[Date],
		t.[Name] = s.[Name],
		t.[Sum] = s.[Sum],
		t.Memo = s.Memo
	when not matched by target then insert
		(Id, [Date], [Name], [Sum], [Memo]) values
		(s.Id, s.[Date], s.[Name], s.[Sum], s.[Memo])
	output inserted.Id into @rtable(Id);

	select top(1) @Id = Id from @rtable;

	merge a2test.[Rows] as t
	using @Rows as s
	on t.Id = s.Id
	when matched then update set
		t.[Memo] = s.Memo,
		t.Qty = s.Qty,
		t.Price = s.Price,
		t.[Sum] = s.[Sum]
	when not matched by target then insert
		(Id, Document, [Memo], Qty, Price, [Sum]) values
		(s.Id, @Id, s.[Memo], s.Qty, s.Price, s.[Sum])
	when not matched by source and Document = @Id then delete;

	commit tran;

	exec a2test.[BatchModel.Load] @TenantId, @UserId, @Id;
end
go
------------------------------------------------
create or alter procedure a2test.[Batch.Proc1]
@TenantId bigint = null,
@UserId bigint = null,
@Id bigint,
@Delta float
as
begin
	set nocount on;
	set transaction isolation level read committed;

	update a2test.Rows set Qty = Qty + @Delta where Id=@Id;
end
go
------------------------------------------------
create or alter procedure a2test.[Batch.Throw]
@TenantId bigint = null,
@UserId bigint = null,
@Id bigint
as
begin
	set nocount on;
	set transaction isolation level read committed;
	throw 60000, N'SQL', 0;
end
go
------------------------------------------------
create or alter procedure a2test.[ParamTypes.Load]
@NVarChar nvarchar(255) = null,
@VarChar varchar(255) = null,
@ByteArray varbinary(max) = null,
@Money money = null,
@Real float = null,
@Date date = null,
@Time time = null,
@DateTime datetime = null,
@Boolean bit = null
as
begin
	set nocount on;
	select [Result!TModel!Object] = null, 
		[NVarChar] = @NVarChar, [VarChar] = @VarChar, [Money] = @Money,
		[Real] = @Real, [Date] = @Date, [Time] = @Time, [DateTime] = @DateTime,
		[ByteArray] = @ByteArray, Boolean = @Boolean
end
go
------------------------------------------------
create or alter procedure a2test.[ParamTypes.ExecLoad]
@NVarChar nvarchar(255) = null,
@VarChar varchar(255) = null,
@ByteArray varbinary(max) = null,
@Money money = null,
@Real float = null,
@Date date = null,
@Time time = null,
@DateTime datetime = null,
@Boolean bit = null
as
begin
	set nocount on;
	select [NVarChar] = @NVarChar, [VarChar] = @VarChar, [Money] = @Money,
		[Real] = @Real, [Date] = @Date, [Time] = @Time, [DateTime] = @DateTime,
		[ByteArray] = @ByteArray, Boolean = @Boolean
end
go
------------------------------------------------
create or alter procedure a2test.[Nullable.SaveModel.Metadata]
as
begin
	set nocount on;
	declare @Document [a2test].[Document.TableType];
	select [Document!Document!Metadata] = null, * from @Document;
end
go
------------------------------------------------
create procedure a2test.[Nullable.SaveModel.Update]
	@TenantId int = null,
	@UserId bigint = null,
	@Document [a2test].[Document.TableType] readonly
as
begin
	set nocount on;
	select [Document!TDocument!Object] = null, [Id!!Id] = Id, [Name!!Name] = [Name],
	NameIsNull = cast(case when [Name] is null then 1 else 0 end as bit)
	from @Document;
end
go
------------------------------------------------
create procedure a2test.[Fallback.Metadata]
as
begin
	set nocount on;
	declare @Document [a2test].[Document.TableType];
	declare @Rows [a2test].[Row.TableType];

	select [Document!Document!Metadata]=null, * from @Document;
	select [Rows!Document.Rows!Metadata]=null, * from @Rows;
end
go
------------------------------------------------
create or alter procedure a2test.[Fallback.Update]
@Document [a2test].[Document.TableType] readonly,
@Rows [a2test].[Row.TableType] readonly
as
begin
	set nocount on;
	declare @Id bigint = null;

	/*
	declare @xml nvarchar(max);
	set @xml = (select * from @Rows for xml auto);
	throw 60000, @xml, 0;
	*/

	select @Id = Id from @Document;

	select [Document!TDocument!Object] = null, [Id!!Id] = Id, [Rows!TRow!Array] = null
		from @Document;

	select [!TRow!Array] = null, [!TDocument.Rows!ParentId]=@Id, 
		[Id!!Id] = Id, [Nested.Key!TNested!] = [Nested.Key], [Nested.Value!TNested!] = [Nested.Value]
	from @Rows
	order by Id;
end
go

------------------------------------------------
create or alter procedure a2test.[ClrTypes.LoadListItemList]
as
begin
	set nocount on;

	select StringValue = N'String 1', Int32Value = 22, Int32ValueNull = 22, Severity = N'Warning', SeverityNull = N'Info'
	union all
	select StringValue = N'String 2', Int32Value = 33, Int32ValueNull = 33, Severity = N'Error', SeverityNull = null
	union all
	select StringValue = null, Int32Value = null, Int32ValueNull = null, Severity = null, SeverityNull = null
end
go

------------------------------------------------
create or alter procedure a2test.[ClrTypes.LoadListItem]
as
begin
	set nocount on;
	select StringValue = N'String 1', Int32Value = 22, Int32ValueNull = null, Severity = N'Warning', SeverityNull = null;
end
go
------------------------------------------------
create or alter procedure a2test.[ClrTypes.LoadListItemG]
as
begin
	set nocount on;
	select StringValue = N'String 1', TValue = 22, TValueNull = null, TValueNull2 = cast(1 as smallint), Severity = N'Warning', SeverityNull = null;
end
go
------------------------------------------------
create or alter procedure a2test.[ClrTypes.LoadJson]
as
begin
	set nocount on;
	select StringValue = N'String 1', Int32Value = 22, Int32ValueNull = null, Severity = N'Warning', 
		SeverityNull = null, [Json]= N'{"strval":"string", "numval": 22, "dblval": 7.5, "boolval": true}';
end
go
------------------------------------------------
create or alter procedure a2test.[ClrTypes.LoadBinary]
as
begin
	set nocount on;

	declare @vb varbinary(max);
	set @vb = convert(varbinary(max), N'0x255044462D312E370D0A25E2E3CFD30D0A312030206F626A0D0A3C3C0D0A2F54797065202F436174616C6F670D');
	select [Name] = N'VarBinary', Stream = @vb, ByteArray = @vb, StreamNull = null, ByteArrayNull = null, [Length]=datalength(@vb);
end
go
------------------------------------------------
create or alter procedure a2test.[MultiplyTrees.Load]
	@TenantId int = null,
	@UserId bigint = null,
	@Id bigint = null
as
begin
	set nocount on;
	select [Model!TModel!Array] = null, [Id!!Id] = 5, [Name]=N'Tree 5', [Elements!TElem!Array] = null
	union all
	select [Model!TModel!Array] = null, [Id!!Id] = 7, [Name]=N'Tree 7', [Elements!TElem!Array] = null;

	declare @tx table([Order] int, [Root] int, Id int, Parent int, [Name] nvarchar(255));
	insert into @tx([Order], [Root], Id, Parent, [Name]) values
		(0, 5,   50, null, N'5-50'),
		(0, 5,   51, null, N'5-51'),
		(10, 7,   70, null, N'7-70'),
		(20, null, 500, 50,  N'5-50-500'),
		(30, null, 510, 50,  N'5-50-510'),
		(40, null, 700, 70,  N'7-70-700'),
		(50, null, 710, 700,  N'7-70-700-710');

	select [!TElem!Tree] = null, [Id!!Id] = Id, [Name], [!TElem.TreeItems!ParentId] = Parent,
		[!TModel.Elements!ParentId] = [Root],
		[TreeItems!TElem!Items] = null
	from @tx
	order by [Order]
end
go
------------------------------------------------
create or alter procedure a2test.[ChildrenTree.Load]
	@TenantId int = null,
	@UserId bigint = null,
	@Id bigint = null
as
begin
	set nocount on;
	select [Model!TModel!Object] = null, [Id!!Id] = 5, [Name]=N'Tree 5', [Elements!TElem!Array] = null

	declare @tx table([Order] int, [Root] int, Id int, Parent int, [Name] nvarchar(255));
	insert into @tx([Order], [Root], Id, Parent, [Name]) values
		(0, 5,   50, null, N'5-50'),
		(0, 5,   51, null, N'5-51'),
		(20, null, 500, 50,  N'5-50-500'),
		(30, null, 510, 50,  N'5-50-510'),
		(50, null, 5100, 510,  N'5-50-510-5100');

	select [!TElem!Tree] = null, [Id!!Id] = Id, [Name], [!TElem.TreeItems!ParentId] = Parent,
		[!TModel.Elements!ParentId] = [Root],
		[TreeItems!TElem!Items] = null
	from @tx
	order by [Order]
end
go
------------------------------------------------
create or alter procedure a2test.SetTenantId
@TenantId int 
as
begin
	set nocount on;
	exec sp_set_session_context @key=N'TenantId', @value=@TenantId, @read_only=0;
end
go
------------------------------------------------
create or alter procedure a2test.[TestTenant.Load]
as
begin
	set nocount on;
	select [Elem!TElem!Object] = null, TenantId = cast(session_context(N'TenantId') as int)
end
go
------------------------------------------------
drop procedure if exists a2test.[Expando.Tables];
drop type if exists [a2test].[Expando.TableType];
go
------------------------------------------------
create type [a2test].[Expando.TableType] as
table (
	[Id] bigint null,
	[Name] nvarchar(255),
	Bool bit
)
go
------------------------------------------------
create or alter procedure a2test.[Expando.Tables]
@Id bigint,
@Name nvarchar(255),
@Elements [a2test].[Expando.TableType] readonly
as
begin
	set nocount on;

	/*
	declare @xml nvarchar(max);
	set @xml = (select * from @Elements for xml auto);
	throw 60000, @xml, 0;
	*/

	select Id=@Id, [Name]=@Name, [Count] = count(*), [Sum] = sum(Id), [Text]=STRING_AGG([Name], N',')
	from @Elements;
end
go
------------------------------------------------
create or alter procedure a2test.[Expando.Simple]
@Id bigint,
@Name nvarchar(255),
@Number float
as
begin
	set nocount on;

	select Id=@Id, [Name]=@Name, [Number] = @Number;
end
go
------------------------------------------------
create or alter procedure a2test.[ChildMapObject.Load]
as
begin
	set nocount on;
	select [Model!TModel!Object] = null, [Id!!Id] = 5, [Agent!TAgent!RefId]=7;

	select [!TAgent!Map] = null, [Id!!Id] = 7, [Name] = N'AgentName', 
		[AgChild!TChild!Object] = null;

	select [!TChild!Object] = null, [Id!!Id] = 284, [Name] = 'Child', [!TAgent.AgChild!ParentId] = 7;
end
go
------------------------------------------------
create or alter procedure a2test.[ChildMapArray.Load]
as
begin
	set nocount on;

	select [Agents!TAgent!Array] = null, [Id!!Id] = 7, [Name] = N'Agent1', 
		[AgChild!TChild!Object] = null
	union all
	select [Agents!TAgent!Array] = null, [Id!!Id] = 8, [Name] = N'Agent2', 
		[AgChild!TChild!Object] = null;

	select [!TChild!Object] = null, [Id!!Id] = 284, [Name] = 'Child', [!TAgent.AgChild!ParentId] = 7;
end
go
------------------------------------------------
drop procedure if exists a2test.[Document.Guids.Metadata];
drop procedure if exists a2test.[Document.Guids.Update];
drop type if exists [a2test].[Document.Guids.TableType];
drop type if exists [a2test].[Document.GuidsRows.TableType];
go
------------------------------------------------
create type [a2test].[Document.Guids.TableType] as
table (
	[Id] bigint null,
	Agent uniqueidentifier
)
go
------------------------------------------------
create type [a2test].[Document.GuidsRows.TableType] as
table (
	[Id] bigint null,
	Item uniqueidentifier
)
go
------------------------------------------------
create or alter procedure a2test.[Document.Guids.Metadata]
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	declare @Document [a2test].[Document.Guids.TableType];
	declare @Rows [a2test].[Document.GuidsRows.TableType];

	select [Document!Document!Metadata] = null, * from @Document;
	select [Rows!Document.Rows!Metadata] = null, * from @Rows;
end
go
------------------------------------------------
create or alter procedure a2test.[Document.Guids.Update]
@TenantId int = null,
@UserId bigint = null,
@Document [a2test].[Document.Guids.TableType] readonly,
@Rows [a2test].[Document.GuidsRows.TableType] readonly
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	/*
	declare @xml nvarchar(max);
	set @xml = (select * from @Document for xml auto);
	throw 60000, @xml, 0;
	*/
	select [Document!TDocument!Object] = null, [Id!!Id] = Id, [Agent], [Rows!TRow!Array] = null
	from @Document;

	declare @id bigint;
	select @id = Id from @Document;

	select [!TRow!Array] = null, [Id!!Id] = Id, [Item], [!TDocument.Rows!ParentId] = @id
	from @Rows;
end
go

------------------------------------------------
create procedure a2test.[EmptyString.Metadata]
as
begin
	set nocount on;
	declare @Document [a2test].[Document.TableType];
	select [Document!Document!Metadata]=null, * from @Document;
end
go
------------------------------------------------
create or alter procedure a2test.[EmptyString.Update]
@TenantId int = null,
@UserId bigint = null,
@Document [a2test].[Document.TableType] readonly
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	/*
	declare @xml nvarchar(max);
	set @xml = (select * from @Document for xml auto);
	throw 60000, @xml, 0;
	*/
	select [Document!TDocument!Object] = null, [Id!!Id] = Id,
		[Name], [String] = case when [Name] is null then N'NULL' else N'EMPTY' end
	from @Document
end
go

------------------------------------------------
create procedure a2test.[Agent.SameProps.Metadata]
as
begin
	set nocount on;
	declare @Agent [a2test].[Id.TableType];
	declare @Tag [a2test].[Tag.TableType];
	select [Agent!Agent!Metadata]=null, * from @Agent;
	select [Tags!Agent.Tags!Metadata]=null, * from @Tag;
	select [SubTags!Agent.Tags.SubTags!Metadata]=null, * from @Tag;
end
go
------------------------------------------------
create or alter procedure a2test.[Agent.SameProps.Update]
@TenantId int = null,
@UserId bigint = null,
@Agent [a2test].[Id.TableType] readonly,
@Tags [a2test].[Tag.TableType] readonly,
@SubTags [a2test].[Tag.TableType] readonly
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	/*
	declare @xml nvarchar(max);
	set @xml = (select * from @SubTags for xml auto);
	throw 60000, @xml, 0;
	*/

	declare @id bigint;
	select @id = Id from @Agent;
	select [Agent!TAgent!Object] = null, [Id!!Id] = Id,
		[Tags!TTag!Array] = null
	from @Agent;

	select [!TTag!Array] = null, [Id!!Id] = Id, [!TAgent.Tags!ParentId] = @id,
		[SubTags!TTag!Array] = null
	from @Tags;

	select [!TTag!Array] = null, [Id!!Id] = Id, [!TTag.SubTags!ParentId] = ParentId
	from @SubTags;
end
go
-----------------------------------------------
create or alter procedure a2test.[ScalarTypes.Metadata]
as
begin
	set nocount on;
	declare @Obj [a2test].[ScalarTypes.TableType];
	select [Obj!MainObject!Metadata] = null, * from @Obj;

end
go
-----------------------------------------------
create or alter procedure a2test.[ScalarTypes.Update]
@Obj [a2test].[ScalarTypes.TableType] readonly
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	select [MainObject!TMain!Object] = null,
		Int32Value, Int64Value, StringValue, MoneyValue, FloatValue, BitValue,
		GuidValue,	DateTimeValue
	from @Obj;
end
go
-------------------------------------------------
create or alter procedure a2test.[Blob.Update]
@TenantId int = 1,
@UserId bigint,
@Name nvarchar(255), 
@Mime nvarchar(255),
@Stream varbinary(max),
@BlobName nvarchar(255)
as
begin
	set nocount on;
	set transaction isolation level read committed;

	set @TenantId = isnull(@TenantId, 1); -- required for image

	declare @rtable table(id bigint, token uniqueidentifier);
	insert into @rtable(id, token) values (123, newid());

	select Id = id, Token = token, [Stream] = @Stream, [Name] = @Name, [Mime] = @Mime from @rtable;
end
go
-------------------------------------------------
create or alter procedure a2test.[DynamicGrouping.Index]
@UserId bigint = null
as
begin
	set nocount on;

	declare @trans table(Id bigint, [Date] date, Agent bigint, Entity bigint, Qty float, Price float);
	declare @agents table(Id bigint, [Name] nvarchar(255));
	declare @entities  table(Id bigint, [Name] nvarchar(255));

	declare @cross table(Parent bigint, [Key] nvarchar(20), [Value] float);

	insert into @trans(Id, [Date], [Agent], Entity, Qty, Price) values
		(20, N'20230101', 100, 20, 1, 5),
		(21, N'20230102', 100, 21, 5, 9),
		(22, N'20230201', 101, 20, 3, 5),
		(30, N'20230205', 101, 22, 8, 4);

	insert into @agents(Id, [Name]) values
		(100, N'Agent 100'),
		(101, N'Agent 101');

	insert into @entities(Id, [Name]) values
		(20, N'Product 20'),
		(21, N'Product 21'),
		(22, N'Product 22');

	insert into @cross(Parent, [Key], [Value]) values
		(20, N'K1', 10),
		(20, N'K2', 20),
		(22, N'K1', 30),
		(22, N'K2', 40);

	select [Trans!TTrans!Array] = null, [Id!!Id] = Id, [Date], 
		[Agent!TAgent!RefId] = Agent, 
		[Entity!TEntity!RefId] = Entity, 
		Qty, Price, [Sum] = cast(Qty * Price as money),
		[Items!TTrans!Items] = null, -- array for nested elements,
		[Cross1!TCross!CrossArray] = null
	from @trans
	order by [Date];

	select [!TAgent!Map] = null, [Id!!Id] = Id, [Name]
	from @agents;

	select [!TEntity!Map] = null, [Id!!Id] = Id, [Name]
	from @entities;

	select [!TCross!CrossArray] = null, [Key!!Key] = [Key], [Value], [!TTrans.Cross1!ParentId] = Parent
	from @cross;


	select [Trans!$Grouping!] = null, [Property] = N'Agent', Func = 'Group'
	union all
	select [Trans!$Grouping!] = null, [Property] = N'Entity', Func = 'Group'
	union all
	select [Trans!$Grouping!] = null, [Property] = N'Sum', Func = N'Sum'
	union all
	select [Trans!$Grouping!] = null, [Property] = N'Price', Func = N'Avg'
	union all
	select [Trans!$Grouping!] = null, [Property] = N'Qty', Func = N'Count'

end
go
-------------------------------------------------
create or alter procedure a2test.[DynamicGrouping2.Index]
@UserId bigint = null,
@Group nvarchar(255)
as
begin
	set nocount on;

	declare @trans table(Id bigint, [Company] bigint, Warehouse bigint, Item bigint, [Sum] money);
	declare @warehouses table(Id bigint, [Name] nvarchar(255));
	declare @companies  table(Id bigint, [Name] nvarchar(255));
	declare @items table(Id bigint, [Name] nvarchar(255));

	declare @cross table(Parent bigint, [Key] nvarchar(20), [Sum] money, Qty float);

	insert into @trans(Id, [Warehouse], Company, Item, [Sum]) values
		(172, 100,  100, 108,   23),
		(173, 100,  100, 100,   12),
		(177, 100,  100, 109, 1500),
		(178, 100,  100, 108,  580),
		(179, 100,  100, 100,  240);

	insert into @companies(Id, [Name]) values
		(100, N'Company 1');

	insert into @warehouses(Id, [Name]) values
		(100, N'Warehouse 1');

	insert into @items(Id, [Name]) values
		(100, N'Product 1'),
		(108, N'Product 2'),
		(109, N'Product 3');

	insert into @cross(Parent, [Key], [Sum], Qty) values
		(172, N'631', 23, 5),
		(173, N'631', 12, 2),
		(177, N'631', 1500, 15),
		(178, N'631', 580, 7.2),
		(179, N'631', 240, 12.5);

	select [Trans!TTrans!Array] = null, [Id!!Id] = Id,
		[Company!TCompany!RefId] = Company, 
		[Warehouse!TWarehouse!RefId] = Warehouse, 
		[Item!TItem!RefId] = Item, 
		[Sum],
		[Cross1!TCross!CrossArray] = null,
		[Items!TTrans!Items] = null -- array for nested elements,
	from @trans
	order by Id;

	select [!TCompany!Map] = null, [Id!!Id] = Id, [Name]
	from @companies;

	select [!TWarehouse!Map] = null, [Id!!Id] = Id, [Name]
	from @warehouses;

	select [!TItem!Map] = null, [Id!!Id] = Id, [Name]
	from @items;

	select [!TCross!CrossArray] = null, [Key!!Key] = [Key], [Sum], Qty, [!TTrans.Cross1!ParentId] = Parent
	from @cross;

	if @Group = N'All'
		select [Trans!$Grouping!] = null, [Property] = N'Company', Func = 'Group'
		union all
		select [Trans!$Grouping!] = null, [Property] = N'Warehouse', Func = 'Group'
		union all
		select [Trans!$Grouping!] = null, [Property] = N'Item', Func = 'Group'
		union all
		select [Trans!$Grouping!] = null, [Property] = N'Sum', Func = N'Sum'
		union all
		select [Trans!$Grouping!] = null, [Property] = N'Cross1#Sum', Func = N'Cross'
		union all
		select [Trans!$Grouping!] = null, [Property] = N'Cross1#Qty', Func = N'Cross';

	else if @Group = N'Item'
		select [Trans!$Grouping!] = null, [Property] = N'Company', Func = 'None'
		union all
		select [Trans!$Grouping!] = null, [Property] = N'Warehouse', Func = 'None'
		union all
		select [Trans!$Grouping!] = null, [Property] = N'Item', Func = 'Group'
		union all
		select [Trans!$Grouping!] = null, [Property] = N'Sum', Func = N'Sum'
		union all
		select [Trans!$Grouping!] = null, [Property] = N'Cross1#Sum', Func = N'Cross'
		union all
		select [Trans!$Grouping!] = null, [Property] = N'Cross1#Qty', Func = N'Cross';

	else if @Group = N'Company'
		select [Trans!$Grouping!] = null, [Property] = N'Company', Func = 'Group'
		--union all
		--select [Trans!$Grouping!] = null, [Property] = N'Warehouse', Func = 'None'
		--union all
		--select [Trans!$Grouping!] = null, [Property] = N'Item', Func = 'None'
		union all
		select [Trans!$Grouping!] = null, [Property] = N'Sum', Func = N'Sum'
		union all
		select [Trans!$Grouping!] = null, [Property] = N'Cross1#Sum', Func = N'Cross'
		union all
		select [Trans!$Grouping!] = null, [Property] = N'Cross1#Qty', Func = N'Cross';

	else if @Group = N'None'
		--select [Trans!$Grouping!] = null, [Property] = N'Company', Func = 'Group'
		--union all
		--select [Trans!$Grouping!] = null, [Property] = N'Warehouse', Func = 'None'
		--union all
		--select [Trans!$Grouping!] = null, [Property] = N'Item', Func = 'None'
		--union all
		select [Trans!$Grouping!] = null, [Property] = N'Sum', Func = N'Sum'
		union all
		select [Trans!$Grouping!] = null, [Property] = N'Cross1#Sum', Func = N'Cross'
		union all
		select [Trans!$Grouping!] = null, [Property] = N'Cross1#Qty', Func = N'Cross';
end
go
------------------------------------------------
create or alter procedure a2test.[RawData.Load]
@Id uniqueidentifier
as
begin
	set nocount on;
	select Id = @Id, 23, N'string';

	select Id = @Id, 77, N'item1'
	union all
	select Id = @Id, 99, N'item2'
end
go
------------------------------------------------
create or alter procedure a2test.[LookupModel]
as
begin
	set nocount on;
	select [Methods!TMtdLookup!Lookup] = null, [!!Key] = 23, [Name!!Name] = N'Element 23', [Value] = N'Value 23'
	union all
	select [Methods!TMtdLookup!Lookup] = null, [!!Key] = 34, [Name!!Name] = N'Element 34', [Value] = N'Value 34'
	union all
	select [Methods!TMtdLookup!Lookup] = null, [!!Key] = 45, [Name!!Name] = N'Element 45', [Value] = N'Value 45'
end
go
------------------------------------------------
create or alter procedure a2test.[MultParent.Load]
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;
	select [RepInfo!TRepInfo!Object] = null, [Id!!Id] = 1,
		[Views!TItem!Array] = null, [Grouping!TItem!Array] = null, [Filters!TItem!Array] = null;

	select [!TItem!Array] =  null, [Name] = N'View', [!TRepInfo.Views!ParentId] = 1, [!TRepInfo.Grouping!ParentId] = null, [!TRepInfo.Filters!ParentId] = null
	union all
	select [!TItem!Array] =  null, [Name] = N'Grouping', [!TRepInfo.Views!ParentId] = null, [!TRepInfo.Grouping!ParentId] = 1, [!TRepInfo.Filters!ParentId] = null
	union all
	select [!TItem!Array] =  null, [Name] = N'Filter', [!TRepInfo.Views!ParentId] = null, [!TRepInfo.Grouping!ParentId] = null, [!TRepInfo.Filters!ParentId] = 1
end
go
-------------------------------------------------
create or alter procedure a2test.[DynamicGrouping.Date]
@UserId bigint = null
as
begin
	set nocount on;

	declare @trans table(Id bigint, [Date] date, Agent bigint, Entity bigint, Qty float, Price float);
	declare @agents table(Id bigint, [Name] nvarchar(255));
	declare @entities  table(Id bigint, [Name] nvarchar(255));

	declare @cross table(Parent bigint, [Key] nvarchar(20), [Value] float);

	insert into @trans(Id, [Date], [Agent], Entity, Qty, Price) values
		(20, N'20230101', 100, 20, 1, 5),
		(21, N'20230101', 100, 21, 5, 9),
		(22, N'20230201', 101, 20, 3, 5),
		(30, N'20230201', 101, 22, 8, 4);

	insert into @agents(Id, [Name]) values
		(100, N'Agent 100'),
		(101, N'Agent 101');

	insert into @entities(Id, [Name]) values
		(20, N'Product 20'),
		(21, N'Product 21'),
		(22, N'Product 22');

	select [Trans!TTrans!Array] = null, [Id!!Id] = Id, [Date], 
		[Agent!TAgent!RefId] = Agent, 
		[Entity!TEntity!RefId] = Entity, 
		Qty, Price, [Sum] = cast(Qty * Price as money),
		[Items!TTrans!Items] = null -- array for nested elements
	from @trans
	order by [Date];

	select [!TAgent!Map] = null, [Id!!Id] = Id, [Name]
	from @agents;

	select [!TEntity!Map] = null, [Id!!Id] = Id, [Name]
	from @entities;


	select [Trans!$Grouping!] = null, [Property] = N'Date', Func = 'Group'
	union all
	select [Trans!$Grouping!] = null, [Property] = N'Agent', Func = 'Group'
	union all
	select [Trans!$Grouping!] = null, [Property] = N'Sum', Func = N'Sum'
	union all
	select [Trans!$Grouping!] = null, [Property] = N'Price', Func = N'Avg'
end
go
------------------------------------------------
drop procedure if exists a2test.[List.Save];
drop type if exists a2test.[ScheduledCommand.TableType];
go
------------------------------------------------
create type a2test.[ScheduledCommand.TableType] as table
(
	Command nvarchar(64),
    [Data] nvarchar(1500),
	UtcRunAt datetime
)
go
------------------------------------------------
create or alter procedure a2test.[List.Save]
@Commands a2test.[ScheduledCommand.TableType] readonly
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	delete from a2test.ScheduledCommands;
	insert a2test.ScheduledCommands (Command, [Data], UtcRunAt)
	select Command, [Data], UtcRunAt
	from @Commands;
end
go
------------------------------------------------
create or alter procedure a2test.[List.Load]
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	select Command, [Data], UtcRunAt
	from a2test.ScheduledCommands;
end
go
------------------------------------------------
create or alter procedure a2test.[Filters.Load]
@TenantId int = null,
@UserId bigint = null,
@Date date
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	select [TDocuments!TDocument!Array] = null, [Id!!Id] = Id, [Date], [Company!TCompany!RefId] = 127
	from a2test.Documents;

	select [!TCompany!Map] = null, [Id!!Id] = 127, [Name] = N'Company 127';

	select [!$System!] = null, [!Documents!Offset] = 0, [!Documents!PageSize] = 20, 
		[!Documents!SortOrder] = N'name', [!Documents!SortDir] = N'asc',
		[!Documents.Period.From!Filter] = @Date, [!Documents.Period.To!Filter] = dateadd(day, 1, @Date),
		[!Documents.Agent.Id!Filter] = 15, [!Documents.Agent.Name!Filter] = N'AgentName',
		[!Documents.Fragment!Filter] = N'FRAGMENT',
		[!Documents.Company.TCompany.RefId!Filter] = 127,
		[!Documents.Warehouse.TWarehouse.RefId!Filter] = null;
end
go
------------------------------------------------
create or alter procedure a2test.[Sheet.Model.Load]
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	declare @rows table(Id bigint, [Index] int);
	declare @columns table(Id bigint, [Key] nvarchar(3), [Name] nvarchar(255));

	declare @cells table(Id bigint, [RowId] bigint, ColumnId bigint, [Value] nvarchar(255));


	insert into @rows(Id, [Index]) values
	(100, 1),
	(101, 3),
	(102, 5),
	(103, 7);

	insert into @columns(Id, [Key], [Name]) values
	(200, N'A', N'Column 1'),
	(201, N'C', N'Column 3'),
	(202, N'D', N'Column 4'),
	(204, N'F', N'Column 6');

	insert into @cells(Id, RowId, ColumnId, [Value]) values
	(500, 100, 200, N'A1'),
	(501, 100, 201, N'C1'),
	(502, 101, 201, N'C3');

	select [Model!TModel!Object] = null, [Id!!Id] = 1,
		[Sheet!TSheet!Sheet] = null;

	select [!TSheet!Sheet] = null, [Id!!Id] = 7, 
		[Rows!TRow!Array] = null, [Columns!TColumn!Array] = null,
		[!TModel.Sheet!ParentId] = 1;

	/*Index name is required */
	select [!TRow!Rows] = null, [Id!!Id] = Id, [Index!!Index] = [Index], [!TSheet.Rows!ParentId] = 7,
		[Cells!TCell!Array] = null
	from @rows
	order by [Index];

	/*Key name is reqired*/
	select [!TColumn!Columns] = null, [Id!!Id] = Id, [Key!!Key] = [Key], [Name!!Name] = [Name], [!TSheet.Columns!ParentId] = 7
	from @columns
	order by Id;

	select [!TCell!Cells] = null, [Id!!Id] = Id, [!TRow.Cells!ParentId] = RowId, 
			[!TColumn.Key!ColumnId] = ColumnId,  /* HACK: "Key" part is using for calculate column index (!) */
		[Value]
	from @cells;
end
go

------------------------------------------------
create or alter procedure a2test.[Sheet.ModelRoot.Load]
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	declare @rows table(Id bigint, [Index] int);
	declare @columns table(Id bigint, [Key] nvarchar(3), [Name] nvarchar(255));

	declare @cells table(Id bigint, [RowId] bigint, ColumnId bigint, [Value] nvarchar(255));


	insert into @rows(Id, [Index]) values
	(100, 1),
	(101, 3),
	(102, 5),
	(103, 7);

	insert into @columns(Id, [Key], [Name]) values
	(200, N'A', N'Column 1'),
	(201, N'C', N'Column 3'),
	(202, N'D', N'Column 4'),
	(204, N'F', N'Column 6');

	insert into @cells(Id, RowId, ColumnId, [Value]) values
	(500, 100, 200, N'A1'),
	(501, 100, 201, N'C1'),
	(502, 101, 201, N'C3');

	select [Sheet!TSheet!Sheet] = null, [Id!!Id] = 7, 
		[Rows!TRow!Array] = null, [Columns!TColumn!Array] = null;

	/*Index name is required */
	select [!TRow!Rows] = null, [Id!!Id] = Id, [Index!!Index] = [Index], [!TSheet.Rows!ParentId] = 7,
		[Cells!TCell!Array] = null
	from @rows
	order by [Index];

	/*Key name is reqired*/
	select [!TColumn!Columns] = null, [Id!!Id] = Id, [Key!!Key] = [Key], [Name!!Name] = [Name], [!TSheet.Columns!ParentId] = 7
	from @columns
	order by Id;

	select [!TCell!Cells] = null, [Id!!Id] = Id, [!TRow.Cells!ParentId] = RowId, 
			[!TColumn.Key!ColumnId] = ColumnId,  /* HACK: "Key" part is using for calculate column index (!) */
		[Value]
	from @cells;
end
go

------------------------------------------------
create or alter procedure a2test.[Spreadsheet.Model.Load]
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	--ZZZ999999
	declare @rows table([Index] int);
	declare @columns table([Ref] nvarchar(3), [Name] nvarchar(255));

	declare @cells table([Ref] nvarchar(9), [Value] nvarchar(255));

	insert into @rows([Index]) values
	(1),
	(3),
	(5),
	(7);

	insert into @columns([Ref], [Name]) values
	(N'A', N'Column 1'),
	(N'C', N'Column 3'),
	(N'D', N'Column 4'),
	(N'F', N'Column 6');

	insert into @cells([Ref], [Value]) values
	(N'A1', N'A1 value'),
	(N'C1', N'C1 value'),
	(N'C3', N'C3 value');

	select [Sheet!TSheet!Object] = null, [Id!!Id] = 7, 
		[Rows!TRow!Object] = null, [Columns!TColumn!Object] = null, [Cells!TCell!Object] = null;

	select [!TRow!Object] = null, [!!Prop] = [Index], [Index], [!TSheet.Rows!ParentId] = 7
	from @rows
	order by [Index];

	select [!TColumn!Object] = null, [!!Prop] = [Ref], [Name!!Name] = [Name], [!TSheet.Columns!ParentId] = 7
	from @columns
	order by [Ref];

	select [!TCell!Object] = null, [!!Prop] = Ref, [Value], [!TSheet.Cells!ParentId] = 7
	from @cells;
end
go
------------------------------------------------
drop procedure if exists a2test.[Spreadsheet.Model.Metadata];
drop procedure if exists a2test.[Spreadsheet.Model.Update];
drop type if exists a2test.[Spreadsheet.Sheet.TableType];
drop type if exists a2test.[Spreadsheet.Sheet.Row.TableType];
drop type if exists a2test.[Spreadsheet.Sheet.Column.TableType];
drop type if exists a2test.[Spreadsheet.Sheet.Cell.TableType];
go
------------------------------------------------
create type a2test.[Spreadsheet.Sheet.TableType] as table 
(
	Id int
);
go
------------------------------------------------
create type a2test.[Spreadsheet.Sheet.Row.TableType] as table
(
	ParentId int,
	[Prop] int,
	[Index] int
);
go
------------------------------------------------
create type a2test.[Spreadsheet.Sheet.Column.TableType] as table
(
	ParentId int,
	[Prop] nvarchar(9),
	[Name] nvarchar(255)
);
go
------------------------------------------------
create type a2test.[Spreadsheet.Sheet.Cell.TableType] as table
(
	ParentId int,
	[Prop] nvarchar(9),
	[Value] nvarchar(255)
);
go
------------------------------------------------
create or alter procedure a2test.[Spreadsheet.Model.Metadata]
as
begin
	set nocount on;
	declare @Sheet a2test.[Spreadsheet.Sheet.TableType];
	declare @Rows a2test.[Spreadsheet.Sheet.Row.TableType];
	declare @Columns a2test.[Spreadsheet.Sheet.Column.TableType];
	declare @Cells a2test.[Spreadsheet.Sheet.Cell.TableType];

	select [Sheet!Sheet!Metadata] = null, * from @Sheet;
	select [Rows!Sheet.Rows*!Metadata] = null, * from @Rows;
	select [Columns!Sheet.Columns*!Metadata] = null, * from @Columns;
	select [Cells!Sheet.Cells*!Metadata] = null, * from @Cells;
end
go

------------------------------------------------
create or alter procedure a2test.[Spreadsheet.Model.Update]
@Sheet a2test.[Spreadsheet.Sheet.TableType] readonly,
@Rows a2test.[Spreadsheet.Sheet.Row.TableType] readonly,
@Columns a2test.[Spreadsheet.Sheet.Column.TableType] readonly,
@Cells a2test.[Spreadsheet.Sheet.Cell.TableType] readonly
as
begin
	set nocount on;
	select [Sheet!TSheet!Object] = null, [Id!!Id] = Id,
		[Rows!TRow!Object] = null, [Columns!TColumn!Object] = null, [Cells!TCell!Object] = null
	from @Sheet;

	select [!TRow!Object] = null, [!!Prop] = [Prop], [Index], [!TSheet.Rows!ParentId] = ParentId
	from @Rows;

	select [!TColumn!Object] = null, [!!Prop] = [Prop], [Name!!Name] = [Name], [!TSheet.Columns!ParentId] = ParentId
	from @Columns;

	select [!TCell!Object] = null, [!!Prop] = Prop, [Value], [!TSheet.Cells!ParentId] = ParentId
	from @Cells;
end
go
------------------------------------------------
create or alter procedure a2test.[BlobModel]
as
begin
	set nocount on;
	select [Company!TCompany!Object] = null, [Stream] = CRYPT_GEN_RANDOM(2048);
end
go
