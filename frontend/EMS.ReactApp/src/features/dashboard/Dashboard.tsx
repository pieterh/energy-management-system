
import Box from '@material-ui/core/Box';
import Container from '@material-ui/core/Container';
import Grid from '@material-ui/core/Grid';

import Button from '@material-ui/core/Button';
import Typography from '@material-ui/core/Typography';
import Paper from '@material-ui/core/Paper';
import { makeStyles, createStyles, Theme } from '@material-ui/core/styles';

import { useAppSelector, useAppDispatch } from  '../../common/hooks';
import { pingAsync, selectIsLoggedIn } from '../authentication/authenticationSlice';
import { DashboardCard } from '../../components/dashboardcard/DashboardCard';

import { EVSEInfo, EVSESessionInfo } from '../chargepoint/ChargePointInfo';

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

export default function Main() {
  const classes = useStyles();  
  const isLoggedIn = useAppSelector(selectIsLoggedIn);
  const dispatch = useAppDispatch();
  const bull = <span className={classes.bullet}>•</span>;

  async function onPing(){   
    dispatch(pingAsync()).then((x) =>{

    });
  }

  return(
    <Container>
      {isLoggedIn && 
        <Grid container spacing={3} >

          <Grid item xs={12} sm={12} md={6} lg={3} >
            <EVSEInfo />
          </Grid>

          <Grid item xs={12} sm={12} md={6} lg={3} >
            <EVSESessionInfo />
          </Grid>
          
          <Grid item xs={12} sm={12} md={12} lg={3} >
            <DashboardCard title="tst">
              <Grid container spacing={1}>
                <Grid item xs={6}>
                  <Paper className={classes.paper}>één</Paper>
                </Grid>
                <Grid item xs={6}>
                  <Paper className={classes.paper}>twee</Paper>
                </Grid>              
                <Grid item xs={12}>
                  <Button onClick={onPing} >ping</Button>
                </Grid>
              </Grid>
            </DashboardCard>
          </Grid>

          <Grid item xs={3} >
            <DashboardCard title="1" >
              <Typography className={classes.title} color="textSecondary" gutterBottom>
                  Word of the Day
              </Typography>
              <Typography variant="h5" component="h2">
                  be{bull}nev{bull}o{bull}lent
              </Typography>
              <Typography className={classes.pos} color="textSecondary">
                  adjective
              </Typography>
              <Typography variant="body2" component="p">
                  well meaning and kindly.
                  <br />
                  {'"a benevolent smile"'}
              </Typography>
            </DashboardCard>
          </Grid>
          <Grid item xs={3} >
            <DashboardCard title="2">b</DashboardCard>
          </Grid>

        </Grid>
      }

      {!isLoggedIn && 
        <Grid item xs={12} sm={4}>
            <Box bgcolor="primary.main" color="primary.contrastText" p={2}>
                voor iedereen!yihkkk
            </Box>
        </Grid>
      } 
    </Container>
  )
}
