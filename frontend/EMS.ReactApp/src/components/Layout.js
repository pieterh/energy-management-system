import React from 'react';
import { Link } from 'react-router-dom';
import { Header, Container, Divider, Icon } from 'semantic-ui-react';

import { pullRight, h1 } from './Layout.css';

const Layout = ({ children }) => {
    return (
        <Container>
            <Link to="/">
                <Header as="h1" className={h1}>
                    webpack-for-react
                </Header>
            </Link>
            {children}
            <Divider />
            <p className={pullRight}>
                Testing
            </p>
        </Container>
    );
};

export default Layout;
