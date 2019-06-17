import React from 'react';
import { NotificationListing, NotificationView } from './dashboard-helpers';

class Dashboard extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            notifications: props.notifications,
            currentNotification: props.notifications[0]
        };
    }

    onNotificationSelect(notification) {
        CareerCircleWebApp.updateSubscriberNotification(notification);

        for (const n of this.state.notifications) {
            if (n.notificationGuid === notification.notificationGuid) {
                this.setState({
                    currentNotification: n
                });
            }
        }
        
    };

    render() {
        return (
            <div className="dashboard shadow-2">
                <div className="row">
                    <div className="col-3 no-padding">
                        <NotificationListing notifications={this.state.notifications} onNotificationSelect={(notification) => this.onNotificationSelect(notification)} currentNotification={this.state.currentNotification} />
                    </div>
                    <div className="col-9 no-padding">
                        <NotificationView notification={this.state.currentNotification} />
                    </div>
                </div>
            </div>
            
            
        );
    }
}




export default Dashboard;