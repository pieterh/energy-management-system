import React from 'react';
import { styled } from '@mui/material/styles';

import Card from '@mui/material/Card';
import CardActions from '@mui/material/CardActions';
import CardContent from '@mui/material/CardContent';
import CardHeader from '@mui/material/CardHeader';

const PREFIX = 'DashboardCard';
const classes = {
  root: `${PREFIX}-root`,
  paper: `${PREFIX}-paper`,
  bullet: `${PREFIX}-bullet`,
  title: `${PREFIX}-title`,
  pos: `${PREFIX}-pos`,
}
const Root = styled('div')(({ theme }) => ({
  [`&.${classes.root}`]: {
    flexGrow: 1,
  },
  [`& .${classes.paper}`]: {
    padding: theme.spacing(2),
    textAlign: 'center',
    color: theme.palette.text.secondary,
  },
  [`& .${classes.bullet}`]: {
    display: 'inline-block',
    margin: '0 2px',
    transform: 'scale(0.8)',
  },
  [`& .${classes.title}`]: {
    fontSize: 14,
  },
  [`& .${classes.pos}`]: {
    marginBottom: 12,
  },     
}))


export type IDashboardCardProps = {
  title: string;
  subheader?: string;
  avatar?: React.ReactNode;
  children?: React.ReactNode;
}

export function DashboardCard(props : IDashboardCardProps) { 
  return(
    <React.Fragment>
      <Root>
        <Card variant="outlined">
            <CardHeader
              avatar= { props.avatar }
              // action={
              //   <IconButton aria-label="settings">
              //     <MoreVertIcon />
              //   </IconButton>
              // }
              title={props.title}
              subheader={props.subheader}
            />              
            <CardContent >
              {props.children}
            </CardContent>
        </Card>
      </Root>
    </React.Fragment>
  )
};
