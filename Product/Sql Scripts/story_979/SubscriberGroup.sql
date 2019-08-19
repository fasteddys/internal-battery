--Update all subscribers groupId to default groupId who are assigned to auto generated groups


Update sg
set sg.GroupId=(Select GroupId from [Group] where Name='CareerCircle Organic Signup')
from SubscriberGroup sg
join [Group] g on sg.GroupId=g.GroupId
where g.[Description] like '%Auto-generated group%'

Update SubscriberGroup
set CreatedByGroup=1
where IsDeleted=0