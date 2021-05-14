import { DateTime, DateTimeFormatOptions, Duration } from 'luxon';
import   prettyMilliseconds  from 'pretty-ms';

export enum DurationFormat { Humanized };
export enum DateTimeFormat { Humanized };


export function FormatDurationFromSeconds(seconds: number, f: DurationFormat = DurationFormat.Humanized) : string {
    return FormatDuration(Duration.fromMillis(seconds * 1000), f);
}

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
    return (d.isValid) ? FormatDuration(d, f) : "";
}

export function FormatFromISO(str: string | undefined, f: DateTimeFormat = DateTimeFormat.Humanized) : string {
    if (str == undefined) return "";
    var d = DateTime.fromISO(str);
    return (d.isValid) ? FormatDateTime(d, f) : "";
}

const resolvedOptions = Intl.DateTimeFormat().resolvedOptions();

const options : DateTimeFormatOptions = {
    year: 'numeric', month: 'numeric', day: 'numeric',
    hour: 'numeric', minute: 'numeric',
    hour12: false,
    timeZone: resolvedOptions.timeZone
  };

const dateTimeFormatter = new Intl.DateTimeFormat(resolvedOptions.locale, options);

export function FormatDateTime(date: DateTime, f: DateTimeFormat = DateTimeFormat.Humanized) : string {
    switch (f){
        case DateTimeFormat.Humanized:
            var r = dateTimeFormatter.format(date.toJSDate());
            return r;
            break;
    }
    return "";
}
