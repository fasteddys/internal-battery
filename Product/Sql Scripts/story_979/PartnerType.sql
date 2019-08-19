IF (Select Count(*) from PartnerType where Name='CareerCircle Default Seed')=0
BEGIN
    Insert into PartnerType (IsDeleted,CreateDate, ModifyDate, CreateGuid, ModifyGuid, PartnerTypeGuid, Name, [Description])
    VALUES(0, GETUTCDATE(), null,'00000000-0000-0000-0000-000000000000',null, NEWID(), 'CareerCircle Default Seed','Career Circle Default Seed Partner is assigned to this Partner Type.')
END