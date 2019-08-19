If (Select count(*) from Partner where Name='CareerCircle')=0
BEGIN
    Insert Into Partner (IsDeleted, CreateDate, ModifyDate, CreateGuid, ModifyGuid, PartnerGuid, Name, [Description], LogoUrl, ApiToken, PartnerTypeId)
    VALUES (
        0, 
        GETUTCDATE(), 
        null, 
        '00000000-0000-0000-0000-000000000000', 
        null, 
        NEWID(), 
        'CareerCircle', 
        'This is a default Partner. Assign all leads who do not have a partner to this partner',
        null,
        null,
        (Select PartnerTypeId from PartnerType where Name='CareerCircle Default Seed')
        )
END