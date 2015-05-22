---
exec sp_configure 'show advanced options', '1';
go
reconfigure;
go
exec sp_configure 'clr enabled', '1'
go
reconfigure;
exec sp_configure 'show advanced options', '1';
go


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JoinStr]') AND type = N'AF')
DROP AGGREGATE [dbo].[JoinStr]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JoinStr2]') AND type = N'AF')
DROP AGGREGATE [dbo].[JoinStr2]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[JoinStr3]') AND type = N'AF')
DROP AGGREGATE [dbo].[JoinStr3]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NewBigInt]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].NewBigInt
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Split]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].Split
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSplitItem]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].GetSplitItem
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BitAnd]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[BitAnd]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BitContains]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[BitContains]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BitOr]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[BitOr]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetJsonValue]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetJsonValue]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BigIntToIds]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].BigIntToIds
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetClrSimilar]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].GetClrSimilar
GO

IF  EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'MyCLr' and is_user_defined = 1)
DROP ASSEMBLY [MyCLr]
GO
--===============================================================================================

CREATE ASSEMBLY MyCLr 
FROM   'D:\MyClr.dll'
WITH permission_set = Safe;
GO

CREATE AGGREGATE [dbo].JoinStr(@input nvarchar(max)   )
RETURNS nvarchar(max)
EXTERNAL NAME MyCLr.JoinStr;
go

CREATE AGGREGATE [dbo].JoinStr2(@input nvarchar(max) , @sep nvarchar(5)  )
RETURNS nvarchar(max)
EXTERNAL NAME MyCLr.JoinStr2;
go

CREATE AGGREGATE [dbo].JoinStr3(@input nvarchar(max) , @sep nvarchar(5), @isDesc bit )
RETURNS nvarchar(max)
EXTERNAL NAME MyCLr.JoinStr3;
go

--============================bigint==============================================================

CREATE FUNCTION [dbo].[NewBigInt](@val bigInt )
RETURNS [nvarchar](4000) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [MyClr].[MyClr].[NewBigInt]
go

CREATE FUNCTION [dbo].[BitAnd](@val [nvarchar](4000), @Other [nvarchar](4000))
RETURNS [nvarchar](4000) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [MyClr].[MyClr].[BitAnd]
go

CREATE FUNCTION [dbo].[BitContains](@val [nvarchar](4000), @RowIndex [int])
RETURNS [bit] WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [MyClr].[MyClr].[BitContains]
GO

CREATE FUNCTION [dbo].[BitOr](@val [nvarchar](4000), @Other [nvarchar](4000))
RETURNS [nvarchar](4000) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [MyClr].[MyClr].[BitOr]
GO

CREATE FUNCTION [dbo].BigIntToIds(@val [nvarchar](4000))
RETURNS table(value int ) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [MyClr].[MyClr].BigIntToIds

go
CREATE FUNCTION [dbo].Split(@val [nvarchar](4000),@split nvarchar(10))
RETURNS table(Id int , value nvarchar(4000) ) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [MyClr].[MyClr].Split
GO
CREATE FUNCTION [dbo].GetSplitItem(@val [nvarchar](max),@split nvarchar(10),@index int)
RETURNS nvarchar(max)  WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [MyClr].[MyClr].GetSplitItem
GO

--============================Json==============================================================

CREATE FUNCTION [dbo].[GetJsonValue](@val [nvarchar](max), @key nvarchar(200))
RETURNS [nvarchar](max) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [MyClr].[MyClr].[GetJsonValue]
go
--============================Other==============================================================
CREATE FUNCTION [dbo].[GetClrSimilar](@Value1 [nvarchar](4000), @Value2 [nvarchar](4000))
RETURNS float WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [MyClr].[MyClr].[GetClrSimilar]
GO

/*
使用示例：
1. 查询权限中的行集权限，查看权限，菜单行的值：
	select  dbo.GetJsonValue( [Power],'Row.View.Menu') from T_STANDARD_ROLE
	where CODE = '101'
	－－－－－－－－－－－－－－－－－－
	结果： 1FFFC000,0,0,0,0

2. 把上述结果解析为 行ID集合：
	select * from dbo.BigIntToIds('1FFFC000,0,0,0,0')
	－－－－－－－－－－－－－－－－－－
	143
	144
	145
	146
	147
	148
	149
	150
	151
	152
	153
	154
	155
	156
	157

3. 根据Sql行Id生成新的大数字
	select dbo.[NewBigInt]( 143 )
	－－－－－－－－－－－－－－－－－－
	4000,0,0,0,0
	
	select dbo.[NewBigInt]( 144 )
	－－－－－－－－－－－－－－－－－－
	8000,0,0,0,0
	
4. 对两个大数字进行 OR  操作,权限合并：
	select dbo.[BitOr]('4000,0,0,0,0','8000,0,0,0,0')
	－－－－－－－－－－－－－－－－－－
	C000,0,0,0,0
	
5. 判断大数字是否包含某行的权限：
	select dbo.[BitContains]('4000,0,0,0,0',143)
	－－－－－－－－－－－－－－－－－－
	1

*/
