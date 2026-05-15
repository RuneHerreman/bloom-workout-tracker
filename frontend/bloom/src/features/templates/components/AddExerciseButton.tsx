import Button from "../../../components/general/ButtonComponent.tsx";
import {Plus} from "lucide-react";

function AddExerciseButton({ onClick }: { onClick: () => void }) {
    return(
        <Button text="Add Exercise" style="grey" icon={<Plus size={15}/>} onClick={onClick}/>
    )
}

export default AddExerciseButton;