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


exec a2test.[DynamicGrouping.Index];

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
