import React from 'react';
import { BrowserRouter as Router, Route, Link } from "react-router-dom";
class BlogPost extends React.Component {
    constructor(props) {
        super(props);
        this.showImage = this.showImage.bind(this);
        this.handleClick = this.handleClick.bind(this);
    }
    showImage(post) {
        return (
            <img src={post.featuredImage} width={250} height={150} mode='fit' />
        );
    }

    getTimeDifference(time) {
        
    }

    handleClick()
    {
        this.props.clickHandler(this.props.slug)
    }

    

    render() {
        const { post } = this.props;
        return (
            <div className="d-flex flex-row ">
                <div className="p-2">
                    {this.showImage(post)}
                </div>
                <div className="p-2">
                    <div className="d-flex flex-column">
                        <div className="p-2" style={{ fontSize: '2rem' }}>
                            <Route path='/privacy-policy' component={() => {
                                window.location.href = 'http://localhost:5000/blog/2';
                                return null;
                            }} />
                            <a href="http://localhost:5000/blog/2">{post.title}</a>
                        </div>
                        <div className="p-2">
                            <div className="d-flex flex-row">
                                <div className="p-2">
                                    {post.author.firstName} {post.author.lastName}
                                </div>
                                <span>|</span>
                                <div className="p-2">
                                    <label>Published: </label>{post.published}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        )
    }
}

export default BlogPost;