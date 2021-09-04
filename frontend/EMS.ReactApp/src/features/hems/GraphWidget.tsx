import React, { useEffect, useState } from "react";
import * as time from 'd3-time'
import { timeFormat } from 'd3-time-format'

import { makeStyles, createStyles, Theme } from "@material-ui/core/styles";
import Avatar from "@material-ui/core/Avatar";
import Grid from "@material-ui/core/Grid";
import Typography from "@material-ui/core/Typography";

import { useAppSelector, useAppDispatch } from "../../common/hooks";
import { getHemsInfoAsync, selectMeasurements } from "./hemsSlice";
import { vehicleIsConnected } from "../chargepoint/EVSESlice";
import { DashboardCard } from "../../components/dashboardcard/DashboardCard";
import EvStationIcon from "@material-ui/icons/EvStation";
import { ResponsiveLine, Line, Serie } from '@nivo/line'
import { prependOnceListener } from "process";

const useStyles = makeStyles((theme: Theme) =>
  createStyles({
    root: {
      flexGrow: 1,
    },
    paper: {
      padding: theme.spacing(2),
      textAlign: "center",
      color: theme.palette.text.secondary,
    },
    pos: {
      marginBottom: 12,
    },
  })
);

export interface IGraphWidget {}

export default GraphWidget;
export function GraphWidget(props: IGraphWidget) {
  const classes = useStyles();
  const dispatch = useAppDispatch();
  const measurements = useAppSelector(selectMeasurements);
  const isVehicleConnected = useAppSelector(vehicleIsConnected);
  var p: Serie[] =[];
  const [data, setData] = useState(p);

  useEffect(() => {
    const interval = setInterval(() => {
      dispatch(getHemsInfoAsync());
    }, 1000);
    return () => clearInterval(interval);
  }, []);

  useEffect(() => {
    var prep: Serie[] =[];
    var serie1 :Serie ={ id:"Grid", color: "hsl(59, 70%, 50%)", data:[]};
    var serie2 :Serie ={ id:"Charge", color: "hsl(103, 70%, 50%)",data:[]};
    // var serieL3 :Serie ={ id:"L3", data:[]};
    var t = measurements.slice(Math.max(measurements.length - 30, 0))
    if (t.length > 0){
    var start = Date.parse(t[0].received);
    t.forEach((d) => {
        //var date = (Date.parse(d.received) - start) / 1000;
        var date = new Date(d.received);
        var tim = time.timeMinute.offset(date, 0);
        //var date = d.received;
        serie1.data.push({x: tim, y: d.l1 + d.l2 + d.l3});
        serie2.data.push({x: tim, y: d.cL1 + d.cL2 + d.cL3});
        // serieL3.data.push({x: d.received, y: d.l3.toString()});
    });

     prep.push(serie1);
     prep.push(serie2);
     //prep.push(serieL3);
    setData(prep);
    console.info("update");
    }
  }, [measurements]);

  return (
    <React.Fragment>
      <DashboardCard
        title="HEMS"
        subheader={"charge"}
        avatar={
          <Avatar>
            <EvStationIcon />
          </Avatar>
        }
      >
        <Grid container item xs={12} spacing={1} style={{height: 400}}>
{/*             <ResponsiveLine
                // height={150}
                // width={800}
                data={data}
                colors={{ scheme: 'nivo' }}
                margin={{ top: 50, right: 110, bottom: 50, left: 60 }}
                xScale={{ type: 'time', format: 'native' }}
                yScale={{ type: 'linear', min: -17, max: 25, reverse: false }}
                xFormat="time:%M:%S"
                yFormat=" >-.2f"
                axisTop={null}
                axisRight={null}
                axisBottom={{      
                    tickValues: "every 1 second",             
                    tickSize: 5,
                    tickPadding: 5,
                    tickRotation: 0,
                    format: "%M:%S",
                    legend: 'Time',
                    legendOffset: 36,
                    legendPosition: 'middle'
                }}
                axisLeft={{                                 
                    tickSize: 5,
                    tickPadding: 5,
                    tickRotation: 0,                  
                    legend: 'A',
                    legendOffset: -40,
                    legendPosition: 'middle'
                }}
                enablePointLabel={true}
                pointSize={10}
                pointColor={{ theme: 'background' }}
                pointBorderWidth={2}
                pointBorderColor={{ from: 'serieColor' }}
                pointLabelYOffset={-12}
                enablePoints={false}
                enableGridX={true}
                curve="monotoneX"
                animate={true}
                isInteractive={false}
                enableSlices={false}
                useMesh={true}
                legends={[
                    {
                        anchor: 'bottom-right',
                        direction: 'column',
                        justify: false,
                        translateX: 100,
                        translateY: 0,
                        itemsSpacing: 0,
                        itemDirection: 'left-to-right',
                        itemWidth: 80,
                        itemHeight: 20,
                        itemOpacity: 0.75,
                        symbolSize: 12,
                        symbolShape: 'circle',
                        symbolBorderColor: 'rgba(0, 0, 0, .5)',
                        effects: [
                            {
                                on: 'hover',
                                style: {
                                    itemBackground: 'rgba(0, 0, 0, .03)',
                                    itemOpacity: 1
                                }
                            }
                        ]
                    }
                ]}
            /> */}
        </Grid>
      </DashboardCard>
    </React.Fragment>
  );
}
