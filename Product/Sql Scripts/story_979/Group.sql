--Insert Default group, when ever a subscriber or lead comes from unknown source, we assign them to default group which is tied to default partner

If (Select count(*) from [Group] where Name='CareerCircle Organic Signup')=0
BEGIN
    Insert Into [Group] (IsDeleted, CreateDate, ModifyDate, CreateGuid, ModifyGuid, GroupGuid, Name, [Description], IsLeavable, [Path], PartnerId)
    VALUES(
        0,
        GETUTCDATE(),
        null,
        '00000000-0000-0000-0000-000000000000',
        null,
        NEWID(),
        'CareerCircle Organic Signup',
        'Subscriber coming from unknown source or directly signing up without coming from any campaign is assigned to default group',
        1,
        null,
        (Select PartnerId from Partner where Name='CareerCircle')
    )
END

--Insert IT RWS Intake group for Partner IT RWS

If (Select count(*) from [Group] where Name='IT RWS Intake')=0
BEGIN
    Insert Into [Group] (IsDeleted, CreateDate, ModifyDate, CreateGuid, ModifyGuid, GroupGuid, Name, [Description], IsLeavable, [Path], PartnerId)
    VALUES(
        0,
        GETUTCDATE(),
        null,
        '00000000-0000-0000-0000-000000000000',
        null,
        NEWID(),
        'IT RWS Intake',
        'Subscriber coming from IR RWS source',
        1,
        null,
        (Select PartnerId from Partner where Name='IT RWS')
    )
END

--Delete All Auto Generated Groups
Update [GROUP]
set IsDeleted=1
where [Description] like '%Auto-generated group%';


--Update PartnerId for the Groups which have null PartnerId
Update [Group]
set Name='Clinical RWS Intake', 
DESCRIPTION='Subscriber who has signed up for a CareerCircle account after being retrieved from the Clinical RWS job scrape.',
PartnerId=(Select PartnerId from Partner where Name='Clinical RWS')
where Name='RWS Intake';

Update [Group]
set PartnerId=(Select PartnerId from Partner where Name='Coursera')
where Name='Coursera Signup';

Update [Group]
set PartnerId=(Select PartnerId from Partner where Name='Barnett International')
where Name='Barnett Signup';

Update [Group]
set PartnerId=(Select PartnerId from Partner where Name='Woz U')
where Name='Woz Student';

Update [Group]
set PartnerId=(Select PartnerId from Partner where Name='NEXXT')
where Name='NEXXT Lead Signup';

Update [Group]
set PartnerId=(Select PartnerId from Partner where Name='Clinical Research Fasttrack')
where Name='Clinical Research Fastrack Signup';