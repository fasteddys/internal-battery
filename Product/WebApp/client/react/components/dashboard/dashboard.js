import React from 'react';
import { NotificationListing, NotificationView } from './dashboard-helpers';

class Dashboard extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            notifications: props.notifications,
            deviceType: props.deviceType,
            currentNotification: props.notifications[0],
            activeScreen: props.deviceType === "Mobile" ? "list" : "both"
        };

        this.onNotificationSelect = this.onNotificationSelect.bind(this);
        this.toggleMobileView = this.toggleMobileView.bind(this);
    }

    componentDidMount() {
        this.updateDimensions();
        window.addEventListener("resize", this.updateDimensions.bind(this));
        // If there's any notification that's viewed by default and it's unread, mark it as read.
        if (this.state.notifications[0] && this.state.deviceType !== "Mobile") {
            this.onNotificationSelect(this.state.notifications[0]);
        }
    }

    componentWillUnmount() {
        window.removeEventListener("resize", this.updateDimensions.bind(this));
    }

    updateDimensions() {
        if (window.innerWidth < 768) {
            this.setState({
                deviceType: "Mobile",
                activeScreen: "list"
            });
        }
        else if (window.innerWidth >= 768 && window.innerWidth < 1024) {
            this.setState({
                deviceType: "Tablet",
                activeScreen: "both"
            });
        }
        else if (window.innerWidth >= 1024) {
            this.setState({
                deviceType: "Desktop",
                activeScreen: "both"
            });
        }
    }

    onNotificationSelect(notification) {
        CareerCircleWebApp.subscriberReadNotification(notification);
        for (const n of this.state.notifications) {
            if (n.notificationGuid === notification.notificationGuid) {
                n.hasRead = 1;
                this.setState({
                    currentNotification: n
                });
            }
        }
    }

    toggleMobileView() {
        if (this.state.deviceType === "Mobile") {
            if (this.state.activeScreen === "list") {
                this.setState({ activeScreen: "details" });
            }
            else {
                this.setState({ activeScreen: "list" });
            }
        }
        
    }

    render() {
        return (
            <div className="dashboard shadow-2">
                <div className="row row-eq-height">
                    <NotificationListing notifications={this.state.notifications} deviceType={this.state.deviceType} activeScreen={this.state.activeScreen} toggleMobileView={this.toggleMobileView} activeScreen={this.state.activeScreen} onNotificationSelect={(notification) => this.onNotificationSelect(notification)} currentNotification={this.state.currentNotification} />
                    <NotificationView notification={this.state.currentNotification} activeScreen={this.state.activeScreen} toggleMobileView={this.toggleMobileView} activeScreen={this.state.activeScreen} />                
                </div>
            </div>
            
            
        );
    }
}




export default Dashboard;