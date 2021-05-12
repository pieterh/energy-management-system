import { DateTime, Duration } from 'luxon';
import   prettyMilliseconds  from 'pretty-ms';

export enum DurationFormat { Humanized };
export enum DateTimeFormat { Humanized };



export function FormatDuration(duration: Duration, f: DurationFormat = DurationFormat.Humanized) : string {
    switch (f){
        case DurationFormat.Humanized:
            var t = prettyMilliseconds(Math.abs(duration.toMillis()), {compact:true});
            return t;
    }
    return "";
}

export function FormatFromISODiffNow(str: string | undefined, f: DurationFormat = DurationFormat.Humanized) : string {
    if (str == undefined) return "";
    var d = DateTime.fromISO(str).diffNow();
    return (d.isValid)  ?  FormatDuration(d, f) : "";
}

export function FormatDateTime(d: DateTime, f: DateTimeFormat = DateTimeFormat.Humanized) : string {
    switch (f){
        case DateTimeFormat.Humanized:
            break;
    }
    return "";
}
