import React from 'react';
import { NotificationListing, NotificationView } from './dashboard-helpers';

class Dashboard extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            notificationEmailsEnabled: props.notificationEmailsEnabled,
            notifications: props.notifications,
            deviceType: props.deviceType,
            currentNotification: props.notifications[0],
            activeScreen: props.deviceType === "Mobile" ? "list" : "both"
        };

        this.onNotificationSelect = this.onNotificationSelect.bind(this);
        this.onNotificationDelete = this.onNotificationDelete.bind(this);
        this.toggleMobileView = this.toggleMobileView.bind(this);
        this.toggleNotificationEmailsEnabled = this.toggleNotificationEmailsEnabled.bind(this);
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

    onNotificationDelete(notification) {
        CareerCircleWebApp.subscriberDeleteNotification(notification);
        this.setState(prevState => {
            const updatedNotifications = prevState.notifications.filter(n => n.notificationGuid !== notification.notificationGuid);
            const updatedCurrentNotification = updatedNotifications[0];
            return { notifications: updatedNotifications, currentNotification: updatedCurrentNotification };
        });
    }

    toggleNotificationEmailsEnabled() {
        var updatedNotificationEmailEnabled = !this.state.notificationEmailsEnabled;
        if (CareerCircleWebApp.toggleNotificationEmails(updatedNotificationEmailEnabled)) {
            this.setState({
                notificationEmailsEnabled: updatedNotificationEmailEnabled
            });
        }
        else {
            ToastService.error('Unable to modify email notification state.', 'Oops, Something went wrong.');
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
                <div className="row settings">
                    <div className="col-12">
                        <label className="pointer no-select">
                            <Checkbox isChecked={this.state.notificationEmailsEnabled} onChange={this.toggleNotificationEmailsEnabled} />
                            {!this.state.notificationEmailsEnabled
                                ? <span className="favorites-label"><i className="far fa-square"></i> Email reminders are disabled</span>
                                : <span className="favorites-label"><i className="far fa-check-square"></i> Email reminders are enabled</span>
                            }
                        </label>
                    </div>
                </div>
                <div className="row row-eq-height">
                    <NotificationListing notifications={this.state.notifications} deviceType={this.state.deviceType} activeScreen={this.state.activeScreen} toggleMobileView={this.toggleMobileView} activeScreen={this.state.activeScreen} onNotificationSelect={(notification) => this.onNotificationSelect(notification)} currentNotification={this.state.currentNotification} />
                    <NotificationView notification={this.state.currentNotification} activeScreen={this.state.activeScreen} toggleMobileView={this.toggleMobileView} activeScreen={this.state.activeScreen} onNotificationDelete={(notification) => this.onNotificationDelete(notification)} />
                </div>
            </div>
        );
    }
}




export default Dashboard;