
import React from 'react';
import Container from '@mui/material/Container';
import Typography from '@mui/material/Typography';

import Link from '@mui/material/Link';
import { styled } from '@mui/material/styles';

const PREFIX = 'MyCard';
const classes = {
  root: `${PREFIX}-root`,
  content: `${PREFIX}-content`,
}
const Root = styled('div')(({ theme }) => ({
      [`&.${classes.root}`]: {
        display: 'flex',
        alignItems: 'center',
        backgroundColor: theme.palette.primary.main
      },
      [`& .${classes.content}`]: {
        color: theme.palette.common.white,
        fontSize: 16,
        lineHeight: 1.7
      },
    }))
    

interface ICreditsProps {
  //classes: any
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
            <Root className={classes.root}> 
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
            </Root>
        );
    }
}

export default Credits;

