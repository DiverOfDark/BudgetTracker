import moment from 'moment';
import 'moment/locale/ru'
moment.locale('ru')

export function formatMoney(from: number) {
    return from.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$& ');
}

export function formatUnixMonth(from: number) {
    return moment.unix(from).format("MMMM YYYY");
}

export function formatMonth(from: string) {
    return moment(from).format("MMMM YYYY");
}

export function formatUnixDate(from: number) {
    return moment.unix(from).format("DD.MM.YYYY");
}

export function formatUnixTime(from: number) {
    return moment.unix(from).format("DD.MM.YY H:mm:ss");
}

export function formatDate(from: string) {
    return moment(from).format("DD.MM.YYYY");
}

export function formatDateJs(from: string) {
    return moment(from).format("YYYY-MM-DD");
}

export function formatDateTime(from: string) {
    return moment(from).format("DD.MM.YYYY HH:mm:ss");
}

export function formatPaymentKind(kind: any) {
    if (kind == 0)
        return "Трата";
    if (kind == 1)
        return "Доход";
    if (kind == 2)
        return "Перевод";
    return "Неизвестно";
}

export function compare(a: any,b: any) {
    if (!a && !b)
        return 0;

    if (a && !b)
        return 1;

    if (!a && b)
        return -1;

    if (a == b)
        return 0;
    if (a > b)
        return 1;
    if (a < b)
        return -1;

    return 0;
}