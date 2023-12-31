﻿CREATE
or ALTER VIEW [dbo].[view_DimDate] AS
SELECT *
FROM OPENROWSET (
		BULK N'https://placeholder_storageaccount.dfs.core.windows.net/placeholder_container/placeholder_datarootpath/Ontologies/Commerce/DimDate/*.parquet',
		FORMAT = 'parquet'
	) WITH (
		[DateId] 			bigint,
		[FullDate] 			date,
		[Year] 				nvarchar(4),
		[YearOrder] 			int,
		[YearStart] 			date,
		[YearEnd] 			date,
		[Quarter] 			nvarchar(7),
		[QuarterOrder] 			bigint,
		[QuarterOfYear] 		nvarchar(2),
		[QuarterOfYearOrder] 		bigint,
		[Month] 			nvarchar(20),
		[MonthOrder] 			int,
		[MonthOfYear] 			nvarchar(20),
		[MonthOfYearShort] 		nvarchar(3),
		[MonthOfYearOrder] 		int,
		[MonthStart] 			date,
		[MonthEnd] 			date,
		[WeekOfYear] 			int,
		[DayOfWeek] 			nvarchar(10),
		[DayOfWeekShort] 		nvarchar(3),
		[DayOfWeekOrder] 		int,
		[DayOfMonth] 			int,
		[DayOfYear] 			int,
		[FiscalCalendar] 		nvarchar(10),
		[FiscalYear] 			nvarchar(10),
		[FiscalYearOrder] 		int,
		[FiscalYearStartDate] 		date,
		[FiscalYearEndDate] 		date,
		[FiscalQuarter] 		nvarchar(7),
		[FiscalQuarterOrder] 		bigint,
		[FiscalQuarterOfYear] 		nvarchar(2),
		[FiscalQuarterOfYearOrder] 	bigint,
		[FiscalPeriod] 			nvarchar(100),
		[FiscalPeriodOrder] 		bigint,
		[FiscalPeriodOfYear] 		nvarchar(60),
		[FiscalPeriodOfYearShort] 	nvarchar(8),
		[FiscalPeriodOfYearOrder] 	bigint,
		[FiscalPeriodStartDate] 	date,
		[FiscalPeriodEndDate] 		date,
		[FiscalDayOfPeriod] 		int,
		[FiscalWeekOfPeriod] 		int,
		[FiscalDayOfWeek] 		int,
		[FiscalWeekStart] 		date,
		[FiscalWeekEnd] 		date,
		[SysProcessingDateTime] 	datetime2,
		[SysIsDeleted] 			int
	) AS [DimDate]