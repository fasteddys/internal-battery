import React from 'react';
class BlogPost extends React.Component {
    constructor(props) {
        super(props);
        this.showImage = this.showImage.bind(this);
    }
    showImage(post) {
        return (
            <img src={post.featuredImage} width={250} height={150} mode='fit' />
        );
    }

    render() {
        const { post } = this.props;
        return (
            <div className="d-flex flex-row">
                <div className="p-2">
                    {this.showImage(post)}
                </div>
                <div className="p-2">
                    <div className="d-flex flex-column">
                        <div className="p-2" style={{fontSize:'2rem'}}>
                            {post.title}
                        </div>
                        <div className="p-2">
                            <div className="d-flex flex-row">
                                <div className="p-2">
                                    {post.author.firstName} {post.author.lastName}
                                </div>
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