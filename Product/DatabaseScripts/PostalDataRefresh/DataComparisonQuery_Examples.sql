--New Zip Codes
SELECT newdata.* 
FROM [careercircledb].[DataImport].[USA_Canada_ZipCode_LatestData] AS newdata
Left Join (
	Select p.Code, c.Name, s.Code as StateCode, s.Name as stateName 
	from [careercircledb]..[Postal] p
	Join [careercircledb]..[City] c
	On c.CityId = p.CityId
	Join [careercircledb]..[State] s
	On s.StateId = c.StateId
	Join [careercircledb]..[Country] co
	On co.CountryId = s.CountryId
	Where co.Code3 in ('USA','CAN')
	) as olddata
ON olddata.Code = newdata.PostalCode
Where olddata.Code is null




--New States
SELECT distinct provincename, countryname 
FROM [careercircledb].[DataImport].[USA_Canada_ZipCode_LatestData] AS newdata
where 
--provincename not in (select name from [careercircledb]..[State])
provinceAbbr not in (select code from [careercircledb]..[State])

SELECT * 
FROM [careercircledb].[DataImport].[USA_Canada_ZipCode_LatestData] AS newdata
Left Join [careercircledb]..[State] s
On s.code = newdata.provinceAbbr
Where s.code is null


--New Cities Names
SELECT distinct cityname, provincename, countryname
FROM [careercircledb].[DataImport].[USA_Canada_ZipCode_LatestData] AS newdata
where cityname not in (select name from [careercircledb]..[city])


select name from [careercircledb]..[city] where stateid = 102


select count(cityName) From [careercircledb].[DataImport].[USA_Canada_ZipCode_LatestData]
where cityname = 'Medicine Hat'


Select count(c.name)
	from [careercircledb]..[Postal] p
	Join [careercircledb]..[City] c
	On c.CityId = p.CityId
	Join [careercircledb]..[State] s
	On s.StateId = c.StateId
	Join [careercircledb]..[Country] co
	On co.CountryId = s.CountryId
	Where co.Code3 in ('USA','CAN')
	and c.name = 'Medicine Hat'


Select count(c.name), c.name as CityName,  s.Name StateName
	from [careercircledb]..[Postal] p
	Join [careercircledb]..[City] c
	On c.CityId = p.CityId
	Join [careercircledb]..[State] s
	On s.StateId = c.StateId
	Join [careercircledb]..[Country] co
	On co.CountryId = s.CountryId
	Where co.Code3 in ('USA','CAN')
group by c.name, s.name
order by count(c.name) desc