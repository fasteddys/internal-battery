import React from 'react';

/* Notications Listing */
export const NotificationListing = ({ notifications, deviceType, onNotificationSelect, currentNotification, activeScreen, toggleMobileView }) => {
    let notificationsList = notifications.map((item, i) => {
        if (i === 0 && deviceType !== "Mobile") {
            item.hasRead = 1;
        }
        return <NotificationItem key={item.notificationGuid} notification={item} toggleMobileView={toggleMobileView} onNotificationSelect={onNotificationSelect} selected={item.notificationGuid === currentNotification.notificationGuid} />
    });

    let classes = "col-12 col-sm-3 no-padding";
    if (activeScreen === "details") {
        classes += " hidden";
    }

    if (notifications.length > 0) {
        return (

            <div className={classes}>
                <div className="notification-listing-container">
                    <ul>
                        {notificationsList}
                    </ul>
                </div>
            </div>
        );
    }
    else {
        return (
            <div className={classes}>
                <div className="notification-listing-container">
                    <ul>
                        <li>No new notifications.</li>
                    </ul>
                </div>
            </div>
            
        );
    }
}

export const ReadableDateTime = (props) => {
    const monthNames = ["January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];
    const notificationDate = new Date(props.date);
    const curr_date = notificationDate.getDate();
    const curr_month = notificationDate.getMonth() + 1; //Months are zero based
    const curr_year = notificationDate.getFullYear();
    const outputDate = monthNames[curr_month - 1] + " " + curr_date + ", " + curr_year;
    return (<span>{outputDate}</span>)
    
};

const NotificationItem = ({ notification, selected, onNotificationSelect, toggleMobileView }) => {

    let classes = selected ? "selected" : "";
    let unreadDot = notification.hasRead === 0 ? <div className="unread-dot"><i className="fas fa-circle"></i></div> : "";

    const onSelect = () => {
        onNotificationSelect(notification);
        toggleMobileView();
        unreadDot = "";
    };

    
    return (<li key={notification.notificationGuid} className={classes} onClick={onSelect}>
        {notification.title}
        {unreadDot}
        <div><span className="notification-date"><ReadableDateTime date={notification.createDate} /></span></div>
    </li>);
}

export const NotificationView = ({ isHidden, toggleView, notification, activeScreen, toggleMobileView }) => {
    

    let classes = "col-12 col-sm-9 no-padding";
    if (activeScreen === "list") {
        classes += " hidden";
    }

    let showBackButton = activeScreen !== "both" ? <div onClick={toggleMobileView}><i className="fas fa-angle-left"></i><i className="fas fa-angle-left"></i> Back</div> : "";

    if (notification) {
        return (
            <div className={classes}>
                <div className="notification-view-container">
                    {showBackButton}
                    <div className="header-container">
                        <h5>{notification.title}</h5>
                        <span className="notification-date"><ReadableDateTime date={notification.createDate} /></span>
                    </div>
                    <div>{notification.description}</div>
                </div>
            </div>
                
        );
    }
    else {
        return (
            <div className={classes}>
                <div className="notification-view-container">
                    <div>Check back later for more CareerCircle notifications!</div>
                </div>
            </div>
                
        );
    }
}