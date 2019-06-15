import { writable } from 'svelte/store';

export let text = writable("");
export let show = writable(false);
export let position = writable({left: 0, top: 0, width: 0, height: 0});

export function tooltip(node: any, newText: string) {
    var currentText: string;

    currentText = newText;
    let currentlyShown = false;

    let mouseover = () => {
        position.set(node.getBoundingClientRect());
        text.set(currentText);
        show.set(true);
        currentlyShown = true;
    }
    
    let mouseout = () => {
        show.set(false);
        currentlyShown = false;
    }

    node.addEventListener('mouseover', mouseover);
    node.addEventListener('mouseout', mouseout);

    return {
        update(newText: string) {
            currentText = newText;
        },

        destroy() {
            if (currentlyShown) {
                show.set(false);
            }
            node.removeEventListener('mouseover', mouseover);
            node.removeEventListener('mouseout', mouseout);
        }
    };
}