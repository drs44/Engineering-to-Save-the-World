﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Drills {
    /**
     * Behavior for containers to place dragged objects into. This container can
     * only take one dragged object.
     */
    public class BlockContainer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        // Function types for actions performed during drop and pick up.
        public delegate void OnBlockAction();

        // Function called when dragged object is placed in block.
        public OnBlockAction OnBlockPlaced;

        // Function called when dragged object is picked up from block.
        public OnBlockAction OnBlockRemoved;

        // Highlight color when dragged object is over block.
        [SerializeField]
        private Color highlightColor;

        // Original color of this container block.
        private Color defaultColor;

        // Renderer for this container block.
        private CanvasRenderer blockRenderer;

        // Whether the current block is full. 
        private bool isFilled = false;

        // Transform for this container block.
        private RectTransform blockTransform;

        // Drag-droppeds object contained by this block, if any.
        private DragDropSnapInto containedObject;

        // Drag-dropped hovering object if any.
        private DragDropSnapInto hoverObject;

        // Use this for initialization
        void Start()
        {
            blockRenderer = GetComponent<CanvasRenderer>();
            defaultColor = blockRenderer.GetColor();
            blockTransform = gameObject.transform as RectTransform;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (containedObject == null)
            {
                blockRenderer.SetColor(highlightColor);
                blockRenderer.SetAlpha(1);
            }
            if (eventData.pointerDrag == null)
            {
                return;
            }
            DragDropSnapInto dragdrop = eventData.pointerDrag.GetComponent<DragDropSnapInto>();
            if (!isFilled && dragdrop != null)
            {
                hoverObject = dragdrop;
                dragdrop.returnPosition = transform.position;
                dragdrop.returnParent = transform;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (containedObject == null)
            {
                blockRenderer.SetColor(defaultColor);
            }
            if (eventData != null && eventData.pointerDrag != null)
            {
                hoverObject = eventData.pointerDrag.GetComponent<DragDropSnapInto>();
            }
            if (hoverObject != null)
            {
                hoverObject.returnPosition = hoverObject.originalPos;
                hoverObject.returnParent = hoverObject.originalParent;
                hoverObject = null;
            }
        }

        // Get component which object can snap into, or null if no such component
        // exists.
        public bool Intersects2D(DragDrop dragObject)
        {
            RectTransform dragTransform = dragObject.GetComponent<RectTransform>();
            return Intersect2D(dragTransform, blockTransform);
        }

        // Adds a contained object if the container is not filled. Returns whether
        // the object was added.
        public bool AddContainedObject(DragDropSnapInto dragObject)
        {
            if (!isFilled)
            {
                containedObject = dragObject;
                isFilled = true;
                blockRenderer.SetColor(dragObject.GetComponent<CanvasRenderer>().GetMaterial().color);
                if (OnBlockPlaced != null)
                {
                    OnBlockPlaced();
                }
                return true;
            }
            return false;
        }

        // Removes the contained object of this block.
        public void RemoveContainedObject()
        {
            blockRenderer.SetColor(defaultColor);
            isFilled = false;
            containedObject = null;
            if (OnBlockRemoved != null)
            {
                OnBlockRemoved();
            }
        }

        // Returns whether the bounds intersect in 2D space.
        static bool Intersect2D(RectTransform rt1, RectTransform rt2)
        {
            Rect r1 = GetWorldRect(rt1);
            Rect r2 = GetWorldRect(rt2);
            // return r1.Overlaps(r2);
            return true;
        }

        static Rect GetWorldRect(RectTransform rt)
        {
            Vector3[] worldCorners = new Vector3[4];
            rt.GetWorldCorners(worldCorners);
            float width = worldCorners[4].x - worldCorners[0].x;
            float height = worldCorners[1].y - worldCorners[0].y;
            return new Rect(worldCorners[0].x, worldCorners[0].y, width, height);
        }
    }
}