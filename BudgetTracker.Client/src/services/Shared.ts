export function formatMoney(from: number) {
    return from.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$& ');
}