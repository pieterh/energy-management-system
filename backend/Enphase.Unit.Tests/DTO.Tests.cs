using System.Text.Json;
using System.Xml.Serialization;
using EMS.Library.Xml;
using Enphase.DTO;
using Enphase.DTO.Home;
using Enphase.DTO.Info;


namespace Enphase.Unit.Tests;

public class UnitTest1
{
    [Fact]
    public void Test0()
    {
        exampleProduction1.Should().NotBeNull();
        exampleInventory1.Should().NotBeNull();
    }

    [Fact]
    public void Test1()
    {
        var r = JsonSerializer.Deserialize<HomeResponse>(exampleHome1);
        r.Should().NotBeNull();
        Assert.NotNull(r);
    }

    [Fact]
    public void Test2()
    {
        var ro = XmlHelpers.XmlToObject<InfoResponse>(infoXml);
        ro.Should().NotBeNull();
        ro.Time.Should().Be(1684191453);
        ro.Device.Should().NotBeNull();
        ro.Packages.Should().NotBeNull();
        ro.Packages.Should().HaveCount(11);
        ro.BuildInfo.Should().NotBeNull();

        //http://192.168.1.8/installer/setup/home
        //Digest qop="auth", realm="enphaseenergy.com", nonce="/4hiZFzh3L1216WtaN90ulrnF/s="
        //Digest username="installer", realm="enphaseenergy.com", nonce="/4hiZFzh3L1216WtaN90ulrnF/s=", uri="/installer/setup/home", response="b314342428bc2c1cdc4fac405bc601d8", qop=auth, nc=00000002, cnonce="6b2639bc7092593b"

        //cookie
        //sessionId=16311da30e1bd00ea4063a8e5a3d3c28; expires=Tue, 16 May 2023 19:33:40 GMT; path=/
        //sessionId=16311da30e1bd00ea4063a8e5a3d3c28; envoy_122011110962={"target_count":15,"target_acb_count":0,"target_nsr_count":0}

        //get power 
        //http://192.168.1.8/ivp/mod/603980032/mode/power
        //{powerForcedOff: false}     powerForcedOff    :        false    }

        // put power
        //http://192.168.1.8/ivp/mod/603980032/mode/power
        //application/x-www-form-urlencoded; charset=UTF-8
        //{"length":1,"arr":[1]}
        //on
        //{"length":1,"arr":[0]}

        //firmware 7
        //https://envoy.local/ivp/peb/devstatus
    }

    [Fact]
    public void Test3()
    {
        var r = JsonSerializer.Deserialize<Inverter[]>(exampleInverters1);
        r.Should().NotBeNullOrEmpty();
        r.Should().HaveCount(15);
        Assert.NotNull(r);
        r[0].SerialNumber.Should().NotBeNullOrWhiteSpace();
        r[0].DeviceType.Should().Be(1);
        r[0].LastResportDate.Should().BeGreaterThan(1672570800);
        r[0].LastReportWatts.Should().BeGreaterThan(1);
        r[0].MaxReportWatts.Should().BeGreaterThan(1);
    }

        //http://192.168.1.8/api/v1/production/inverters
        readonly string exampleInverters1 =
"""
[
  {
    "serialNumber": "121915022126",
    "lastReportDate": 1684227761,
    "devType": 1,
    "lastReportWatts": 163,
    "maxReportWatts": 255
  },
  {
    "serialNumber": "121915017916",
    "lastReportDate": 1684227767,
    "devType": 1,
    "lastReportWatts": 162,
    "maxReportWatts": 255
  },
  {
    "serialNumber": "121915021715",
    "lastReportDate": 1684227758,
    "devType": 1,
    "lastReportWatts": 164,
    "maxReportWatts": 254
  },
  {
    "serialNumber": "121915021492",
    "lastReportDate": 1684227765,
    "devType": 1,
    "lastReportWatts": 166,
    "maxReportWatts": 256
  },
  {
    "serialNumber": "121915018017",
    "lastReportDate": 1684227771,
    "devType": 1,
    "lastReportWatts": 163,
    "maxReportWatts": 255
  },
  {
    "serialNumber": "121915021555",
    "lastReportDate": 1684227771,
    "devType": 1,
    "lastReportWatts": 163,
    "maxReportWatts": 254
  },
  {
    "serialNumber": "121915017742",
    "lastReportDate": 1684227778,
    "devType": 1,
    "lastReportWatts": 164,
    "maxReportWatts": 253
  },
  {
    "serialNumber": "121915017316",
    "lastReportDate": 1684227779,
    "devType": 1,
    "lastReportWatts": 161,
    "maxReportWatts": 253
  },
  {
    "serialNumber": "121915021557",
    "lastReportDate": 1684227776,
    "devType": 1,
    "lastReportWatts": 167,
    "maxReportWatts": 254
  },
  {
    "serialNumber": "121915022509",
    "lastReportDate": 1684227761,
    "devType": 1,
    "lastReportWatts": 161,
    "maxReportWatts": 252
  },
  {
    "serialNumber": "121915021636",
    "lastReportDate": 1684227766,
    "devType": 1,
    "lastReportWatts": 166,
    "maxReportWatts": 255
  },
  {
    "serialNumber": "121915018003",
    "lastReportDate": 1684227774,
    "devType": 1,
    "lastReportWatts": 163,
    "maxReportWatts": 255
  },
  {
    "serialNumber": "121915017571",
    "lastReportDate": 1684227763,
    "devType": 1,
    "lastReportWatts": 159,
    "maxReportWatts": 252
  },
  {
    "serialNumber": "121915017488",
    "lastReportDate": 1684227770,
    "devType": 1,
    "lastReportWatts": 158,
    "maxReportWatts": 253
  },
  {
    "serialNumber": "121915020962",
    "lastReportDate": 1684227775,
    "devType": 1,
    "lastReportWatts": 162,
    "maxReportWatts": 254
  }
]
""";

    readonly string exampleInventory1 =
"""
[{
    "type": "PCU",
    "devices": [{
        "part_num": "800-00630-r02",
        "installed": "1588068812",
        "serial_num": "121915021715",
        "device_status": ["envoy.global.ok"],
        "last_rpt_date": "1684227120",
        "admin_state": 1,
        "dev_type": 1,
        "created_date": "1588068812",
        "img_load_date": "1635248400",
        "img_pnum_running": "520-00082-r01-v04.28.07",
        "ptpn": "540-00134-r01-v04.28.02",
        "chaneid": 1627390225,
        "device_control": [{
            "gficlearset": false
        }],
        "producing": true,
        "communicating": true,
        "provisioned": true,
        "operating": false
    }, {
        "part_num": "800-00630-r02",
        "installed": "1588068816",
        "serial_num": "121915022509",
        "device_status": ["envoy.global.ok"],
        "last_rpt_date": "1684227122",
        "admin_state": 1,
        "dev_type": 1,
        "created_date": "1588068816",
        "img_load_date": "1635248400",
        "img_pnum_running": "520-00082-r01-v04.28.07",
        "ptpn": "540-00134-r01-v04.28.02",
        "chaneid": 1627390481,
        "device_control": [{
            "gficlearset": false
        }],
        "producing": true,
        "communicating": true,
        "provisioned": true,
        "operating": false
    }, {
        "part_num": "800-00630-r02",
        "installed": "1588068820",
        "serial_num": "121915022126",
        "device_status": ["envoy.global.ok"],
        "last_rpt_date": "1684227124",
        "admin_state": 1,
        "dev_type": 1,
        "created_date": "1588068820",
        "img_load_date": "1635248400",
        "img_pnum_running": "520-00082-r01-v04.28.07",
        "ptpn": "540-00134-r01-v04.28.02",
        "chaneid": 1627390737,
        "device_control": [{
            "gficlearset": false
        }],
        "producing": true,
        "communicating": true,
        "provisioned": true,
        "operating": false
    }, {
        "part_num": "800-00630-r02",
        "installed": "1588068824",
        "serial_num": "121915017571",
        "device_status": ["envoy.global.ok"],
        "last_rpt_date": "1684227125",
        "admin_state": 1,
        "dev_type": 1,
        "created_date": "1588068824",
        "img_load_date": "1635248400",
        "img_pnum_running": "520-00082-r01-v04.28.07",
        "ptpn": "540-00134-r01-v04.28.02",
        "chaneid": 1627390993,
        "device_control": [{
            "gficlearset": false
        }],
        "producing": true,
        "communicating": true,
        "provisioned": true,
        "operating": false
    }, {
        "part_num": "800-00630-r02",
        "installed": "1588068831",
        "serial_num": "121915021492",
        "device_status": ["envoy.global.ok"],
        "last_rpt_date": "1684227126",
        "admin_state": 1,
        "dev_type": 1,
        "created_date": "1588068831",
        "img_load_date": "1635248400",
        "img_pnum_running": "520-00082-r01-v04.28.07",
        "ptpn": "540-00134-r01-v04.28.02",
        "chaneid": 1627391249,
        "device_control": [{
            "gficlearset": false
        }],
        "producing": true,
        "communicating": true,
        "provisioned": true,
        "operating": false
    }, {
        "part_num": "800-00630-r02",
        "installed": "1588068835",
        "serial_num": "121915021636",
        "device_status": ["envoy.global.ok"],
        "last_rpt_date": "1684227128",
        "admin_state": 1,
        "dev_type": 1,
        "created_date": "1588068835",
        "img_load_date": "1635248400",
        "img_pnum_running": "520-00082-r01-v04.28.07",
        "ptpn": "540-00134-r01-v04.28.02",
        "chaneid": 1627391505,
        "device_control": [{
            "gficlearset": false
        }],
        "producing": true,
        "communicating": true,
        "provisioned": true,
        "operating": false
    }, {
        "part_num": "800-00630-r02",
        "installed": "1588068839",
        "serial_num": "121915017916",
        "device_status": ["envoy.global.ok"],
        "last_rpt_date": "1684227129",
        "admin_state": 1,
        "dev_type": 1,
        "created_date": "1588068839",
        "img_load_date": "1635248400",
        "img_pnum_running": "520-00082-r01-v04.28.07",
        "ptpn": "540-00134-r01-v04.28.02",
        "chaneid": 1627391761,
        "device_control": [{
            "gficlearset": false
        }],
        "producing": true,
        "communicating": true,
        "provisioned": true,
        "operating": false
    }, {
        "part_num": "800-00630-r02",
        "installed": "1588068843",
        "serial_num": "121915017488",
        "device_status": ["envoy.global.ok"],
        "last_rpt_date": "1684227130",
        "admin_state": 1,
        "dev_type": 1,
        "created_date": "1588068843",
        "img_load_date": "1635248400",
        "img_pnum_running": "520-00082-r01-v04.28.07",
        "ptpn": "540-00134-r01-v04.28.02",
        "chaneid": 1627392017,
        "device_control": [{
            "gficlearset": false
        }],
        "producing": true,
        "communicating": true,
        "provisioned": true,
        "operating": false
    }, {
        "part_num": "800-00630-r02",
        "installed": "1588068852",
        "serial_num": "121915018017",
        "device_status": ["envoy.global.ok"],
        "last_rpt_date": "1684227132",
        "admin_state": 1,
        "dev_type": 1,
        "created_date": "1588068852",
        "img_load_date": "1635248400",
        "img_pnum_running": "520-00082-r01-v04.28.07",
        "ptpn": "540-00134-r01-v04.28.02",
        "chaneid": 1627392273,
        "device_control": [{
            "gficlearset": false
        }],
        "producing": true,
        "communicating": true,
        "provisioned": true,
        "operating": false
    }, {
        "part_num": "800-00630-r02",
        "installed": "1588068856",
        "serial_num": "121915021555",
        "device_status": ["envoy.global.ok"],
        "last_rpt_date": "1684227133",
        "admin_state": 1,
        "dev_type": 1,
        "created_date": "1588068856",
        "img_load_date": "1635248400",
        "img_pnum_running": "520-00082-r01-v04.28.07",
        "ptpn": "540-00134-r01-v04.28.02",
        "chaneid": 1627392529,
        "device_control": [{
            "gficlearset": false
        }],
        "producing": true,
        "communicating": true,
        "provisioned": true,
        "operating": false
    }, {
        "part_num": "800-00630-r02",
        "installed": "1588068859",
        "serial_num": "121915018003",
        "device_status": ["envoy.global.ok"],
        "last_rpt_date": "1684227134",
        "admin_state": 1,
        "dev_type": 1,
        "created_date": "1588068859",
        "img_load_date": "1635248400",
        "img_pnum_running": "520-00082-r01-v04.28.07",
        "ptpn": "540-00134-r01-v04.28.02",
        "chaneid": 1627392785,
        "device_control": [{
            "gficlearset": false
        }],
        "producing": true,
        "communicating": true,
        "provisioned": true,
        "operating": false
    }, {
        "part_num": "800-00630-r02",
        "installed": "1588068863",
        "serial_num": "121915020962",
        "device_status": ["envoy.global.ok"],
        "last_rpt_date": "1684227136",
        "admin_state": 1,
        "dev_type": 1,
        "created_date": "1588068863",
        "img_load_date": "1635248400",
        "img_pnum_running": "520-00082-r01-v04.28.07",
        "ptpn": "540-00134-r01-v04.28.02",
        "chaneid": 1627393041,
        "device_control": [{
            "gficlearset": false
        }],
        "producing": true,
        "communicating": true,
        "provisioned": true,
        "operating": false
    }, {
        "part_num": "800-00630-r02",
        "installed": "1588068873",
        "serial_num": "121915021557",
        "device_status": ["envoy.global.ok"],
        "last_rpt_date": "1684227137",
        "admin_state": 1,
        "dev_type": 1,
        "created_date": "1588068873",
        "img_load_date": "1635248400",
        "img_pnum_running": "520-00082-r01-v04.28.07",
        "ptpn": "540-00134-r01-v04.28.02",
        "chaneid": 1627393297,
        "device_control": [{
            "gficlearset": false
        }],
        "producing": true,
        "communicating": true,
        "provisioned": true,
        "operating": false
    }, {
        "part_num": "800-00630-r02",
        "installed": "1588068876",
        "serial_num": "121915017742",
        "device_status": ["envoy.global.ok"],
        "last_rpt_date": "1684227139",
        "admin_state": 1,
        "dev_type": 1,
        "created_date": "1588068876",
        "img_load_date": "1635248400",
        "img_pnum_running": "520-00082-r01-v04.28.07",
        "ptpn": "540-00134-r01-v04.28.02",
        "chaneid": 1627393553,
        "device_control": [{
            "gficlearset": false
        }],
        "producing": true,
        "communicating": true,
        "provisioned": true,
        "operating": false
    }, {
        "part_num": "800-00630-r02",
        "installed": "1588068881",
        "serial_num": "121915017316",
        "device_status": ["envoy.global.ok"],
        "last_rpt_date": "1684227141",
        "admin_state": 1,
        "dev_type": 1,
        "created_date": "1588068881",
        "img_load_date": "1635248400",
        "img_pnum_running": "520-00082-r01-v04.28.07",
        "ptpn": "540-00134-r01-v04.28.02",
        "chaneid": 1627393809,
        "device_control": [{
            "gficlearset": false
        }],
        "producing": true,
        "communicating": true,
        "provisioned": true,
        "operating": false
    }]
}, {
    "type": "ACB",
    "devices": []
}, {
    "type": "NSRB",
    "devices": []
}]
""";
    readonly string exampleProduction1 =
"""
{
   "production":[
      {
         "type":"inverters",
         "activeCount":15,
         "readingTime":1684176553,
         "wNow":29,
         "whLifetime":14829530
      },
      {
         "type":"eim",
         "activeCount":0,
         "measurementType":"production",
         "readingTime":1684176558,
         "wNow":-0.121,
         "whLifetime":0.0,
         "varhLeadLifetime":0.0,
         "varhLagLifetime":0.0,
         "vahLifetime":0.0,
         "rmsCurrent":0.003,
         "rmsVoltage":234.515,
         "reactPwr":-0.0,
         "apprntPwr":1.757,
         "pwrFactor":-1.0,
         "whToday":0.0,
         "whLastSevenDays":0.0,
         "vahToday":0.0,
         "varhLeadToday":0.0,
         "varhLagToday":0.0,
         "lines":[
            {
               "wNow":-0.121,
               "whLifetime":0.0,
               "varhLeadLifetime":0.0,
               "varhLagLifetime":0.0,
               "vahLifetime":0.0,
               "rmsCurrent":0.003,
               "rmsVoltage":234.515,
               "reactPwr":-0.0,
               "apprntPwr":1.757,
               "pwrFactor":-1.0,
               "whToday":0,
               "whLastSevenDays":0,
               "vahToday":0,
               "varhLeadToday":0,
               "varhLagToday":0
            }
         ]
      }
   ],
   "consumption":[
      {
         "type":"eim",
         "activeCount":0,
         "measurementType":"total-consumption",
         "readingTime":1684176558,
         "wNow":1.14,
         "whLifetime":0.0,
         "varhLeadLifetime":0.0,
         "varhLagLifetime":0.0,
         "vahLifetime":0.0,
         "rmsCurrent":0.643,
         "rmsVoltage":240.815,
         "reactPwr":-0.0,
         "apprntPwr":154.759,
         "pwrFactor":0.01,
         "whToday":0.0,
         "whLastSevenDays":0.0,
         "vahToday":0.0,
         "varhLeadToday":0.0,
         "varhLagToday":0.0,
         "lines":[
            {
               "wNow":0.941,
               "whLifetime":0.0,
               "varhLeadLifetime":0.0,
               "varhLagLifetime":0.0,
               "vahLifetime":0.0,
               "rmsCurrent":0.296,
               "rmsVoltage":234.631,
               "reactPwr":-0.0,
               "apprntPwr":69.358,
               "pwrFactor":0.01,
               "whToday":0,
               "whLastSevenDays":0,
               "vahToday":0,
               "varhLeadToday":0,
               "varhLagToday":0
            },
            {
               "wNow":-0.575,
               "whLifetime":0.0,
               "varhLeadLifetime":0.0,
               "varhLagLifetime":0.0,
               "vahLifetime":0.0,
               "rmsCurrent":0.489,
               "rmsVoltage":6.197,
               "reactPwr":0.0,
               "apprntPwr":3.032,
               "pwrFactor":-0.19,
               "whToday":0,
               "whLastSevenDays":0,
               "vahToday":0,
               "varhLeadToday":0,
               "varhLagToday":0
            },
            {
               "wNow":0.0,
               "whLifetime":0.0,
               "varhLeadLifetime":0.0,
               "varhLagLifetime":0.0,
               "vahLifetime":0.0,
               "rmsCurrent":0.103,
               "rmsVoltage":7.938,
               "reactPwr":0.0,
               "apprntPwr":0.814,
               "pwrFactor":0.0,
               "whToday":0,
               "whLastSevenDays":0,
               "vahToday":0,
               "varhLeadToday":0,
               "varhLagToday":0
            }
         ]
      },
      {
         "type":"eim",
         "activeCount":0,
         "measurementType":"net-consumption",
         "readingTime":1684176558,
         "wNow":1.261,
         "whLifetime":0.0,
         "varhLeadLifetime":0.0,
         "varhLagLifetime":0.0,
         "vahLifetime":0.0,
         "rmsCurrent":0.64,
         "rmsVoltage":247.115,
         "reactPwr":-0.0,
         "apprntPwr":70.29,
         "pwrFactor":0.01,
         "whToday":0,
         "whLastSevenDays":0,
         "vahToday":0,
         "varhLeadToday":0,
         "varhLagToday":0,
         "lines":[
            {
               "wNow":1.062,
               "whLifetime":0.0,
               "varhLeadLifetime":0.0,
               "varhLagLifetime":0.0,
               "vahLifetime":0.0,
               "rmsCurrent":0.293,
               "rmsVoltage":234.746,
               "reactPwr":-0.0,
               "apprntPwr":68.541,
               "pwrFactor":0.0,
               "whToday":0,
               "whLastSevenDays":0,
               "vahToday":0,
               "varhLeadToday":0,
               "varhLagToday":0
            },
            {
               "wNow":0.199,
               "whLifetime":0.0,
               "varhLeadLifetime":0.0,
               "varhLagLifetime":0.0,
               "vahLifetime":0.0,
               "rmsCurrent":0.245,
               "rmsVoltage":3.487,
               "reactPwr":0.0,
               "apprntPwr":0.852,
               "pwrFactor":1.0,
               "whToday":0,
               "whLastSevenDays":0,
               "vahToday":0,
               "varhLeadToday":0,
               "varhLagToday":0
            },
            {
               "wNow":0.0,
               "whLifetime":0.0,
               "varhLeadLifetime":0.0,
               "varhLagLifetime":0.0,
               "vahLifetime":0.0,
               "rmsCurrent":0.103,
               "rmsVoltage":8.882,
               "reactPwr":0.0,
               "apprntPwr":0.897,
               "pwrFactor":0.0,
               "whToday":0,
               "whLastSevenDays":0,
               "vahToday":0,
               "varhLeadToday":0,
               "varhLagToday":0
            }
         ]
      }
   ],
   "storage":[
      {
         "type":"acb",
         "activeCount":0,
         "readingTime":0,
         "wNow":0,
         "whNow":0,
         "state":"idle"
      }
   ]
}
""";

    readonly string exampleHome1 =
    """
{
   "software_build_epoch":1542074240,
   "is_nonvoy":false,
   "db_size":"24 MB",
   "db_percent_full":"6",
   "timezone":"Europe/Amsterdam",
   "current_date":"05/15/2023",
   "current_time":"20:40",
   "network":{
      "web_comm":true,
      "ever_reported_to_enlighten":true,
      "last_enlighten_report_time":1684175759,
      "primary_interface":"eth0",
      "interfaces":[
         {
            "type":"ethernet",
            "interface":"eth0",
            "mac":"00:1D:C0:71:21:33",
            "dhcp":true,
            "ip":"192.168.1.8",
            "signal_strength":1,
            "signal_strength_max":1,
            "carrier":true
         },
         {
            "signal_strength":0,
            "signal_strength_max":0,
            "type":"wifi",
            "interface":"wlan0",
            "mac":"A8:E2:C1:59:5D:AE",
            "dhcp":true,
            "ip":null,
            "carrier":false,
            "supported":true,
            "present":true,
            "configured":false,
            "status":"connecting"
         }
      ]
   },
   "tariff":"single_rate",
   "comm":{
      "num":15,
      "level":5,
      "pcu":{
         "num":15,
         "level":5
      },
      "acb":{
         "num":0,
         "level":0
      },
      "nsrb":{
         "num":0,
         "level":0
      }
   },
   "alerts":[


   ],
   "update_status":"satisfied"
}
""";


    readonly string infoXml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<envoy_info>
   <time>1684191453</time>
   <device>
      <sn>122011110962</sn>
      <pn>800-00554-r03</pn>
      <software>R4.10.35</software>
      <euaid>4c8675</euaid>
      <seqnum>0</seqnum>
      <apiver>1</apiver>
      <imeter>true</imeter>
   </device>
   <package name="rootfs">
      <pn>500-00001-r01</pn>
      <version>02.00.00</version>
      <build>945</build>
   </package>
   <package name="kernel">
      <pn>500-00011-r01</pn>
      <version>04.00.00</version>
      <build>5bb754</build>
   </package>
   <package name="boot">
      <pn>590-00018-r01</pn>
      <version>02.00.01</version>
      <build>426697</build>
   </package>
   <package name="app">
      <pn>500-00002-r01</pn>
      <version>04.10.35</version>
      <build>6ed292</build>
   </package>
   <package name="devimg">
      <pn>500-00005-r01</pn>
      <version>01.02.186</version>
      <build>d0d70f</build>
   </package>
   <package name="geo">
      <pn>500-00008-r01</pn>
      <version>02.01.22</version>
      <build>06e201</build>
   </package>
   <package name="backbone">
      <pn>500-00010-r01</pn>
      <version>04.10.25</version>
      <build>7b7de5</build>
   </package>
   <package name="meter">
      <pn>500-00013-r01</pn>
      <version>03.02.07</version>
      <build>4c9d48</build>
   </package>
   <package name="agf">
      <pn>500-00012-r01</pn>
      <version>02.02.00</version>
      <build>c00a8f</build>
   </package>
   <package name="security">
      <pn>500-00016-r01</pn>
      <version>02.00.00</version>
      <build>54a6dc</build>
   </package>
   <package name="full">
      <pn>500-00001-r01</pn>
      <version>02.00.00</version>
      <build>945</build>
   </package>
   <build_info>
      <build_id>release-4.10.x-103-Nov-12-18-18:25:06</build_id>
      <build_time_gmt>1542157882</build_time_gmt>
   </build_info>
</envoy_info>
""";
}
