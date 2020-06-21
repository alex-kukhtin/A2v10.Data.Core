-- Copyright © 2008-2018 Alex Kukhtin

/* 20180618-7133 */

/*
Depends on Windows Workflow Foundation scripts.

	SqlWorkflowInstanceStoreSchema.sql
	SqlWorkflowInstanceStoreLogic.sql

	in %WinDir%\Microsoft.NET\Framework64\v4.0.30319\SQL\en
*/

use a2v10test;
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.SCHEMATA where SCHEMA_NAME=N'a2test')
begin
	exec sp_executesql N'create schema a2test';
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'SimpleModel.Load')
	drop procedure a2test.[SimpleModel.Load]
go
------------------------------------------------
create procedure a2test.[SimpleModel.Load]
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
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'ArrayModel')
	drop procedure a2test.ArrayModel
go
------------------------------------------------
create procedure a2test.ArrayModel
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;
	select [Customers!TCustomer!Array] = null, [Id!!Id] = 123, [Name!!Name]='ObjectName', [Amount] = cast(55.1234 as decimal(10, 5));
end
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'ComplexModel')
	drop procedure a2test.ComplexModel
go
------------------------------------------------
create procedure a2test.ComplexModel
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
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'TreeModel')
	drop procedure a2test.TreeModel
go
------------------------------------------------
create procedure a2test.TreeModel
	@TenantId int = null,
	@UserId bigint = null
as
begin
	set nocount on;

	select [Menu!TMenu!Tree]=null, [!!Id] = 10, [!TMenu.Menu!ParentId]=null, [Name!!Name]=N'Item 1',
		[Menu!TMenu!Array] = null
	union all
	select [Menu!TMenu!Tree]=null, [!!Id] = 20, [!TMenu.Menu!ParentId]=null, [Name!!Name]=N'Item 2',
		[Menu!TMenu!Array] = null
	union all
	select [Menu!TMenu!Tree]=null, [!!Id] = 110, [!TMenu.Menu!ParentId]=10, [Name!!Name]=N'Item 1.1',
		[Menu!TMenu!Array] = null
	union all
	select [Menu!TMenu!Tree]=null, [!!Id] = 120, [!TMenu.Menu!ParentId]=10, [Name!!Name]=N'Item 1.2',
		[Menu!TMenu!Array] = null
	union all
	select [Menu!TMenu!Tree]=null, [!!Id] = 1100, [!TMenu.Menu!ParentId]=110, [Name!!Name]=N'Item 1.1.1',
		[Menu!TMenu!Array] = null
end
go

------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'GroupModel')
	drop procedure a2test.GroupModel
go
------------------------------------------------
create procedure a2test.GroupModel
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
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'MapRoot')
	drop procedure a2test.MapRoot
go
------------------------------------------------
create procedure a2test.MapRoot
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
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'EmptyArray')
	drop procedure a2test.EmptyArray
go
------------------------------------------------
create procedure a2test.EmptyArray
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
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'NestedObject.Metadata')
	drop procedure a2test.[NestedObject.Metadata]
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'NestedObject.Update')
	drop procedure a2test.[NestedObject.Update]
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'NewObject.Metadata')
	drop procedure a2test.[NewObject.Metadata]
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'NewObject.Update')
	drop procedure a2test.[NewObject.Update]
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'SubObjects.Metadata')
	drop procedure a2test.[SubObjects.Metadata]
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
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'Document.RowsMethods.Metadata')
	drop procedure a2test.[Document.RowsMethods.Metadata]
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'Document.RowsMethods.Update')
	drop procedure a2test.[Document.RowsMethods.Update]
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'Guid.Metadata')
	drop procedure a2test.[Guid.Metadata]
go
------------------------------------------------
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'Guid.Update')
	drop procedure a2test.[Guid.Update]
go
------------------------------------------------
if exists (select * from sys.types st join sys.schemas ss ON st.schema_id = ss.schema_id where st.name = N'NestedMain.TableType' AND ss.name = N'a2test')
	drop type [a2test].[NestedMain.TableType];
go
------------------------------------------------
if exists (select * from sys.types st join sys.schemas ss ON st.schema_id = ss.schema_id where st.name = N'NestedSub.TableType' AND ss.name = N'a2test')
	drop type [a2test].[NestedSub.TableType];
go
------------------------------------------------
if exists (select * from sys.types st join sys.schemas ss ON st.schema_id = ss.schema_id where st.name = N'NestedSubArray.TableType' AND ss.name = N'a2test')
	drop type [a2test].[NestedSubArray.TableType];
go
------------------------------------------------
if exists (select * from sys.types st join sys.schemas ss ON st.schema_id = ss.schema_id where st.name = N'Document.TableType' AND ss.name = N'a2test')
	drop type [a2test].[Document.TableType];
go
------------------------------------------------
if exists (select * from sys.types st join sys.schemas ss ON st.schema_id = ss.schema_id where st.name = N'Row.TableType' AND ss.name = N'a2test')
	drop type [a2test].[Row.TableType];
go
------------------------------------------------
if exists (select * from sys.types st join sys.schemas ss ON st.schema_id = ss.schema_id where st.name = N'Method.TableType' AND ss.name = N'a2test')
	drop type [a2test].[Method.TableType];
go
------------------------------------------------
if exists (select * from sys.types st join sys.schemas ss ON st.schema_id = ss.schema_id where st.name = N'MethodData.TableType' AND ss.name = N'a2test')
	drop type [a2test].[MethodData.TableType];
go
------------------------------------------------
if exists (select * from sys.types st join sys.schemas ss ON st.schema_id = ss.schema_id where st.name = N'GuidMain.TableType' AND ss.name = N'a2test')
	drop type [a2test].[GuidMain.TableType];
go
------------------------------------------------
if exists (select * from sys.types st join sys.schemas ss ON st.schema_id = ss.schema_id where st.name = N'GuidRow.TableType' AND ss.name = N'a2test')
	drop type [a2test].[GuidRow.TableType];
go
------------------------------------------------
create type [a2test].[NestedSub.TableType] as
table (
	[Id] bigint null,
	[ParentId] bigint null,
	[ParentKey] nvarchar(255) null,
	[Name] nvarchar(255),
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
	[RowNumber] int null
)
go
------------------------------------------------
create type [a2test].[Method.TableType] as
table (
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
create type [a2test].[NestedMain.TableType] as
table (
	[Id] bigint null,
	[Name] nvarchar(255),
	[NumValue] float,
	[BitValue] bit,
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

	select [MainObject!TMainObject!Object] = null, [Id!!Id] = Id, [Name!!Name] = Name,
		NumValue = NumValue, BitValue= BitValue,
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
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_SCHEMA=N'a2test' and ROUTINE_NAME=N'Document.Load')
	drop procedure a2test.[Document.Load]
go
------------------------------------------------
create procedure a2test.[Document.Load]
	@TenantId int = null,
	@UserId bigint = null,
	@Id bigint = null
as
begin
	set nocount on;
	select [Document!TDocument!Object] = null, [Id!!Id]=@Id, [Agent!TAgent!RefId] = 300, 
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
	select [Document!TDocument!Object] = null, [Id!!Id] = 123, [Name!!Name]='Document', [Rows!TRow!Array] = null;

	select [!TRow!Array] = null, [Id!!Id]=null, [!TDocument.Rows!ParentId]=123, [Methods!TMethod!MapObject!Mtd1:Mtd2] = null;

	select [!TMethod!MapObject] = null, [Id!!Id] = 11, [Name!!Name] = N'Method 1', [!TRow.Methods!ParentId] = null, [Key!!Key] = N'Mtd1',
		[Data!TMethodData!Array] = null
	union all
	select [!TMethod!MapObject] = null, [Id!!Id] = 22, [Name!!Name] = N'Method 2', [!TRow.Methods!ParentId] = null, [Key!!Key] = N'Mtd2',
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
exec a2test.[Workflow.Clear.All]
go
