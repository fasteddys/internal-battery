import React from 'react';
import Checkbox from '../Checkbox';

 
class Favorite extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            loading: false,
            jobGuid: props.jobGuid,
            jobPostingFavoriteGuid: (!props.jobPostingFavoriteGuid)  ? false : props.jobPostingFavoriteGuid
        };
    }

    handleCheckboxChange(isChecked) {
        if(this.state.loading)
            return;

        this.setState({loading: true}, () => {
            this.saveChange(isChecked)
                .then((res) => {
                    this.setState({ jobPostingFavoriteGuid: !isChecked ? false : res.data.jobPostingFavoriteGuid });
                })
                .catch((err) => {
                    if(err.response && err.response.data)
                        ToastService.error(err.response.data.description, "Unable to Save Job Posting");
                })
                .finally(() => {
                    this.setState({ loading: false });
                });
        });
    }

    saveChange(isChecked) {
        return isChecked ? CareerCircleAPI.addJobFavorite(this.state.jobGuid) : CareerCircleAPI.deleteJobFavorite(this.state.jobPostingFavoriteGuid);
    }

    render() {
        return (<label className={`pointer no-select favorite-job-checkbox ${this.state.loading ? "loading" : ""}`}>
            <Checkbox isChecked={this.state.jobPostingFavoriteGuid} onChange={(event) => this.handleCheckboxChange(event.target.checked)} />
            { !this.state.jobPostingFavoriteGuid 
                ? <span className="favorites-label"><i className="far fa-star fa-lg fave-icon"></i>   Add to Favorites </span>  
                : <span className="favorites-label"><i className="fas fa-star fa-lg fave-icon"></i>   Favorite </span>   
            }
        </label>);
    }
}

export default Favorite;