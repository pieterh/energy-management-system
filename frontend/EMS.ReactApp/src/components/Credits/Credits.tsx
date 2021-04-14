
import React from 'react';
import Container from '@material-ui/core/Container';
import Typography from '@material-ui/core/Typography';

import Link from '@material-ui/core/Link';
import { createStyles, withStyles, Theme } from '@material-ui/core/styles';

const styles = ({ palette, spacing }: Theme) => createStyles({
  });

interface ICreditsProps {
  classes: any
}
  
interface ICreditsState {
}
  
class Credits extends React.Component<ICreditsProps, ICreditsState> {
    constructor(props: ICreditsProps) {
        super(props);  
        this.state = {
            materialUIclasses: {}
        }
    }
    render () {
        return (
            <Container>
                <Typography variant="body2" color="textSecondary" align="center">
                {'Energy Management System is open source under the '}
                    <Link color="inherit" href="https://github.com/pieterh/energy-management-system/blob/main/LICENSE">
                        BSD 3-Clause License
                    </Link>     
                    {' and is free for commercial use.'}                
                </Typography>
                <Typography variant="body2" color="textSecondary" align="center">
                    {' Copyright (c) 2020, Pieter Hilkemeijer'}
                </Typography>
            </Container>
        );
    }
}

export default withStyles(styles)(Credits)
