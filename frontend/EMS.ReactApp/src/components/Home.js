import React from 'react';
import { Link } from 'react-router-dom';

import Layout from './Layout';

const Home = () => {
    return (
        <Layout>
            <p>Hello World "React and Webpack!"</p>
            <p>
                <Link to="/dynamic">Navigate to Dynamic Page</Link>
            </p>
        </Layout>
    );
};

export default Home;
