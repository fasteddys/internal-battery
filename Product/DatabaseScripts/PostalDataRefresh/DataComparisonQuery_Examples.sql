/*
Below queries just help to compare the new and existing data and spot checking after the merge scripts are executed.
*/

--Below query will list all postal code that is in out DB but missing from the Import file/table
Select p.code as postalcode, c.name as cityname, s.Name as statename
	from [Postal] p
	Join [City] c
	On c.CityId = p.CityId
	Join [State] s
	On s.StateId = c.StateId
	Join [Country] co
	On co.CountryId = s.CountryId
	Where co.Code3 in ('USA','CAN')
	--and c.name = 'Gatineau'
	and p.code not in (select postalcode From [DataImport].[USA_Canada_ZipCode_LatestData])

--Query shows New Zip Codes
SELECT newdata.* 
FROM [DataImport].[USA_Canada_ZipCode_LatestData] AS newdata
Left Join (
	Select p.Code, c.Name, s.Code as StateCode, s.Name as stateName 
	from [Postal] p
	Join [City] c
	On c.CityId = p.CityId
	Join [State] s
	On s.StateId = c.StateId
	Join [Country] co
	On co.CountryId = s.CountryId
	Where co.Code3 in ('USA','CAN')
	) as olddata
ON olddata.Code = newdata.PostalCode
Where olddata.Code is null




--Query shows New States
SELECT distinct provincename, countryname 
FROM [DataImport].[USA_Canada_ZipCode_LatestData] AS newdata
where 
--provincename not in (select name from [State])
provinceAbbr not in (select code from [State])

SELECT * 
FROM [DataImport].[USA_Canada_ZipCode_LatestData] AS newdata
Left Join [State] s
On s.code = newdata.provinceAbbr
Where s.code is null


--Query shows New Cities Names
SELECT distinct cityname, provincename, countryname
FROM [DataImport].[USA_Canada_ZipCode_LatestData] AS newdata
where cityname not in (select name from [city])


--Spot checking the data using below queries

select count(cityName) From [DataImport].[USA_Canada_ZipCode_LatestData]
where cityname = 'Medicine Hat'


Select count(c.name)
	from [Postal] p
	Join [City] c
	On c.CityId = p.CityId
	Join [State] s
	On s.StateId = c.StateId
	Join [Country] co
	On co.CountryId = s.CountryId
	Where co.Code3 in ('USA','CAN')
	and c.name = 'Medicine Hat'


Select count(c.name), c.name as CityName,  s.Name StateName
	from [Postal] p
	Join [City] c
	On c.CityId = p.CityId
	Join [State] s
	On s.StateId = c.StateId
	Join [Country] co
	On co.CountryId = s.CountryId
	Where co.Code3 in ('USA','CAN')
group by c.name, s.name
order by count(c.name) desc
