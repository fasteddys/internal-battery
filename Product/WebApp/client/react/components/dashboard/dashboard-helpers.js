import React from 'react';

/* Notications Listing */
export const NotificationListing = ({ notifications, deviceType, onNotificationSelect, onNotificationDelete, currentNotification, activeScreen, toggleMobileView }) => {
    let notificationsList = notifications.map((item, i) => {
        if (i === 0 && deviceType !== "Mobile") {
            item.hasRead = 1;
        }
        return <NotificationItem key={item.notificationGuid} notification={item} toggleMobileView={toggleMobileView} onNotificationSelect={onNotificationSelect} selected={item.notificationGuid === currentNotification.notificationGuid} />
    });

    let classes = "col-12 col-md-2 no-padding";
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
    let notificationHeader = notification.hasRead === 0 ? <strong>{notification.title}</strong> : notification.title;

    const onSelect = () => {
        onNotificationSelect(notification);
        toggleMobileView();
        unreadDot = "";
    };

    return (<li key={notification.notificationGuid} className={classes} onClick={onSelect}>
        {notificationHeader}
        {unreadDot}
        <div><span className="notification-date"><ReadableDateTime date={notification.createDate} /></span></div>
    </li>);
}
 
export const rawMarkup = (info) => {
    return { __html: info };
}



export const NotificationView = ({ isHidden, toggleView, notification, activeScreen, toggleMobileView, onNotificationDelete }) => {

    let classes = "col-12 col-md-10 no-padding";
    if (activeScreen === "list") {
        classes += " hidden";
    }

    let showBackButton = activeScreen !== "both" ? <div onClick={toggleMobileView}><i className="fas fa-angle-left"></i><i className="fas fa-angle-left"></i> Back</div> : "";

    const onDelete = () => {
        bootbox.confirm({
            message: "Are you sure you want to delete this notification?",
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-success'
                },
                cancel: {
                    label: 'No', 
                    classname: 'btn-danger'
                }
            },
            callback: function (result) {
                if (result) {
                    onNotificationDelete(notification);
                }
            }
        });
    };    

    if (notification) {
        return (
            <div className={classes}>
                <div className="notification-view-container">
                    {showBackButton}
                    <div className="header-container">
                        <div>
                            <h5>{notification.title}</h5>
                            <i className="pointer far fa-window-close" onClick={onDelete}></i>
                        </div>
                        <span className="notification-date"><ReadableDateTime date={notification.createDate} /></span>
                    </div>
                    <div dangerouslySetInnerHTML={rawMarkup( notification.description) }  />        
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