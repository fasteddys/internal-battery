import React from 'react';
import BlogPost from './blog-post';
import Paging from '../Paging';
import { CareerCircleWebAppService} from '../../service/careercircle-webapp-service.js';

class Blog extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            meta: {},
            data: this.props.data
        }

        this.openPost = this.openPost.bind(this);

    }


    openPost(slug)
    {
        CareerCircleWebAppService.OpenBlogPage(slug);
    }

    render() {
        const { next_page, previous_page } = this.state.meta
        return (
            <div>
                <div className="blog-container">
                    {this.state.data.map((post, key) => {
                        return (
                            <BlogPost clickHandler={this.openPost} post={post}></BlogPost>
                        )
                    })}
                    <div className="d-flex justify-content-center">
                        <Paging></Paging>
                    </div>
                </div>

            </div>

        )
    }

}

export default Blog;