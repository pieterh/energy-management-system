import React from 'react';

import Card from '@material-ui/core/Card';
import CardActions from '@material-ui/core/CardActions';
import CardContent from '@material-ui/core/CardContent';
import CardHeader from '@material-ui/core/CardHeader';

import { makeStyles, createStyles, Theme } from '@material-ui/core/styles';

const useStyles = makeStyles((theme: Theme) =>
  createStyles({
    root: {
      flexGrow: 1,
    },
    paper: {
      padding: theme.spacing(2),
      textAlign: 'center',
      color: theme.palette.text.secondary,
    },
    bullet: {
      display: 'inline-block',
      margin: '0 2px',
      transform: 'scale(0.8)',
    },
    title: {
      fontSize: 14,
    },
    pos: {
      marginBottom: 12,
    },
  }),
);

export type IDashboardCardProps = {
  title: string;
  children?: React.ReactNode;
}

export function DashboardCard(props : IDashboardCardProps) {
  const classes = useStyles();
  
  return(
    <React.Fragment>
        <Card variant="outlined">
            <CardHeader
              // avatar={
              //   <Avatar aria-label="recipe" className={classes.avatar}>
              //     R
              //   </Avatar>
              // }
              // action={
              //   <IconButton aria-label="settings">
              //     <MoreVertIcon />
              //   </IconButton>
              // }
              title={props.title}
              // subheader="September 14, 2016"
            />              
            <CardContent >
              {props.children}
            </CardContent>
        </Card>
    </React.Fragment>
  )
};
