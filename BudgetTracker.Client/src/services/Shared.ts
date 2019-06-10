import moment from 'moment';
import 'moment/locale/ru'
moment.locale('ru')

export function formatMoney(from: number) {
    return from.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$& ');
}

export function formatMonth(from: string) {
    return moment(from).format("MMMM YYYY");
}

export function formatDate(from: string) {
    return moment(from).format("DD.MM.YYYY");
}

export function formatDateTime(from: string) {
    return moment(from).format("DD.MM.YYYY HH:mm:ss");
}