create function GetResult(
    @patientid INT = null,
    @patientName VARCHAR(50) = NULL,
    @email VARCHAR(30) = null,
    @DOB Date = null
) 
return table 
as 
return(
    select count(*) as totalcount
    from patient where (@patientid is null OR patientid = @patientid)
        AND (@lastname = lastname)
        AND @DOB = DOB
);


create function GetCount(
    @patientid INT=null,
    @email varchar(50) =null,
    @dob date =null 
)

return INT 
as 
begin
    return(
        select count(*) from patient (@patientid ) 
        from patient where
        (@patientid is null or patientid = @patientid)
        And (@email is null or email = @email)
    );
end;


create function getcount (
    @patientid int = null,
    @email varchar(50) = null,
    @dob date = null
) return table 
as 
return (
    select count(@patientid) from patient
    where (@patientid is null or @patientid = patientid)
    And (@email is null or @email = email)
);


create function getcount (
    @patientid int = null,
    @email nvarchar(50) = null,
    @dob date = null 
) return INT 
as 
begin 
    return (
        select count(patientid) from patient
        where (@patientid is null or patientid = @patientid)
        and (@email is null or email = @email)
    );
end;


create procedure GetResult(
    @patientid INT  = null,
    @gender nvarchar(50) = null,
    @dob date = null 
)
as 
begin
    select count(*) as TotalCount 
    from patient
    where (@patientid is null or patientid = @patientid)
    and (@dob is null or dob=@dob)
end;

EXEC GetResult @gender = 'Male';


create procedure GetResult(
    @gender nvarchar(10) = null 
) as 
begin
    set nocount on;
    select count(*) from patient
    where (@gender is null or gender = @gender) and; 
end;


create procedure GetResult(
    @gender nvarchar(10) = null,
    @totalcount int output
) as begin
    set nocount on;
    select @totalcount = count(*)
    from patient where (@gender is null or gender = @gender)
end;

declare @count INT;
EXEC GetResult @gender = 'male', @totalcount = @cnt output;
print @cnt;

create clusted index IX_PATIENT_PATIENTID
on patient(patientid);

create nonclustered index IX_PATIENT_PATIENTID
on patient(lastname,firstname);


with PaginationPatient as(
    select * from patient
    where (@patientid is null or patientid = @patientid) 
        And (@email is null or email = @email)
        order by patientid 
        offset (@pagesize * (@pageno - 1)) rows 
        fetch next @pagesize rows only; 
)

select p.*,pe.* from patient p join PatientEllry pe 
on p.patientid = pe.patientid



select top 5 p.id, count(v.id) as visit_count from Patients join visits on p.id = v.patient_id 
group by p.id order by visit_count desc;

declare @startdate DATE = '01-01-2025'
declare @enddate DATE = '02-01-2025'

select * from visits where visit_date >= @startdate and visit_date < @enddate;


create trigger trg_AfterInsert 
on Users
after insert as
begin
    salect * from inserted;
end;

