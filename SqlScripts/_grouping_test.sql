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



exec a2test.[DynamicGrouping2.Index] 99, N'All';

/* 
[
   {
    Id: 100, $name: 'Agent 100', $title = 'Agent Title', Qty: 6, 'Sum': 50, $level: 0
    Items: [
	   {
	     Value: 'Product 20', $level: 1
		 Items: [
		    { transaction, $level: 2 }
		 ]
	   },
	   {
	     Value: 'Product 21',
		 Items: [
		    { transaction }
		 ]
	   }
	]
]

i = 0; group = 'Agent.Name'; Eval; CreateGroup if needed


*/

