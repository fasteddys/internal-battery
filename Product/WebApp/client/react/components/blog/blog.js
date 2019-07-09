import React from 'react';
import BlogPost  from './blog-post';

class Blog extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            meta: {},
            data: this.props.data
        }

    }

    render() {
        const { next_page, previous_page } = this.state.meta
        return (
            <div className="blog-container">
                {this.state.data.map((post, key) => {
                    return (
                        <BlogPost post={post}></BlogPost>
                    )
                })}
            </div>
        )
    }

}
export default Blog;