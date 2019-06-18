import React from 'react';

/* Notications Listing */
export const NotificationListing = ({ notifications, onNotificationSelect, currentNotification }) => {
    
    const notificationsList = notifications.map((item, i) => {
        if (i === 0) {
            item.hasRead = 1;
        }
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

export class NotificationItem extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            classes: this.props.selected ? "selected" : "",
            unreadDot: this.props.notification.hasRead === 0 ? <div className="unread-dot"><i className="fas fa-circle"></i></div> : "",
            onNotificationSelect: this.props.onNotificationSelect
        };
        this.onSelect = this.onSelect.bind(this);
    }

    onSelect() {
        this.state.onNotificationSelect(this.props.notification);
        this.setState({ unreadDot: "" });
        
    };


    render() {
        return (<li key={this.props.notification.notificationGuid} className={this.state.classes} onClick={ this.onSelect}>
            {this.props.notification.title}
            {this.state.unreadDot}
            <div><span className="notification-date"><ReadableDateTime date={this.props.notification.createDate} /></span></div>
        </li>);
    }
}

export const NotificationView = ({ notification }) => {
    return (
        <div className="notification-view-container">
            <div className="pt-3 pb-3">
                <h5>{notification.title}</h5>
                <span className="notification-date"><ReadableDateTime date={notification.createDate} /></span>
            </div>
            <div>{notification.description}</div>  
        </div>
    );
}