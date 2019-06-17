/* Notications Listing */
export const NotificationListing = ({ notifications, onNotificationSelect, currentNotification }) => {
    
    const notificationsList = notifications.map((item) => {
        return <NotificationItem key={item.notificationGuid} notification={item} onNotificationSelect={onNotificationSelect} selected={item.notificationGuid === currentNotification.notificationGuid} />
    });

    return (
        <div className="notification-listing-container">
            <ul>
                {notificationsList}
            </ul>
        </div>
    );
};

export const NotificationItem = ({ onNotificationSelect, notification, selected }) => {
    let classes = "notification-list-item";
    if (selected) {
        classes += " selected";
    }

    let unreadDot = "";
    if (notification.hasRead === 0) {
        unreadDot = <div className="unread-dot"><i className="fas fa-circle"></i></div>;
    }

    return (<li key={notification.notificationGuid} selected={selected} className={classes} onClick={() => { onNotificationSelect(notification); }}>
        {notification.title}
        {unreadDot}
    </li>);
};

export const NotificationView = ({ notification}) => {
    return (
        <div className="notification-view-container">
            <div className="pt-3 pb-3"><h5>{notification.title}</h5></div>
            <div>{notification.description}</div>  
        </div>
    );
}