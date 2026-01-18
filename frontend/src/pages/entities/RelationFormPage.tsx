import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { apiClient } from '../../api/client';
import { RelationFormSchema, RelationFormData } from '../../utils/formValidation';
import { useEntity } from '../../hooks/useEntity';
import { EntityFormTemplate } from '../../components/forms/EntityFormTemplate';
import { FormFieldWrapper } from '../../components/forms/FormFieldWrapper';
import { DiscardChangesModal } from '../../components/forms/DiscardChangesModal';
import { hasUnsavedChanges, deepClone } from '../../utils/formHelpers';
import { updateRelationWithCommands } from '../../utils/commandDispatcher';
import './EntityFormPage.css';

interface RelationFormPageProps {
  isEdit?: boolean;
}

export function RelationFormPage({ isEdit = false }: RelationFormPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [showDiscardModal, setShowDiscardModal] = useState(false);

  const { data: existingRelation, isLoading: isLoadingRelation } = useEntity(
    isEdit ? 'relations' : '',
    isEdit ? id : ''
  );

  const {
    register,
    watch,
    handleSubmit: handleReactFormSubmit,
    formState: { errors, isSubmitting },
    setError,
    reset,
  } = useForm<RelationFormData>({
    resolver: zodResolver(RelationFormSchema),
    defaultValues: {
      sourceEntity: '',
      targetEntity: '',
      type: '',
      direction: 'Unidirectional',
      properties: {},
      description: '',
      strength: 'Required',
      cardinality: '1:N',
      confidence: 0.5,
      effectiveFrom: '',
      effectiveTo: '',
    },
  });

  const watchedValues = watch();
  const [originalValues, setOriginalValues] = useState<RelationFormData | null>(null);
  const hasChanges = originalValues ? hasUnsavedChanges(originalValues, watchedValues) : false;

  useEffect(() => {
    if (existingRelation) {
      const normalized: RelationFormData = {
        sourceEntity: (existingRelation as any).sourceEntity || '',
        targetEntity: (existingRelation as any).targetEntity || '',
        type: (existingRelation as any).type || '',
        direction: (existingRelation as any).direction || 'Unidirectional',
        properties: (existingRelation as any).properties || {},
        description: (existingRelation as any).description || '',
        strength: (existingRelation as any).strength || 'Required',
        cardinality: (existingRelation as any).cardinality || '1:N',
        confidence: (existingRelation as any).confidence ?? 0.5,
        effectiveFrom: (existingRelation as any).effective_from || '',
        effectiveTo: (existingRelation as any).effective_to || '',
      };

      reset(normalized);
      setOriginalValues(deepClone(normalized));
    }
  }, [existingRelation, reset]);

  useEffect(() => {
    if (!isEdit) {
      const defaults: RelationFormData = {
        sourceEntity: '',
        targetEntity: '',
        type: '',
        direction: 'Unidirectional',
        properties: {},
        description: '',
        strength: 'Required',
        cardinality: '1:N',
        confidence: 0.5,
        effectiveFrom: '',
        effectiveTo: '',
      };
      setOriginalValues(defaults);
    }
  }, [isEdit]);

  const onSubmit = async (data: RelationFormData) => {
    try {
      let response;

      if (isEdit && id) {
        const result = await updateRelationWithCommands(id, originalValues || {}, data);
        if ((result as any)?.id) {
          navigate(`/entities/relations/${(result as any).id}`);
        }
      } else {
        response = await apiClient.post('/relations', data);
        if (response.data?.id) {
          navigate(`/entities/relations/${response.data.id}`);
        }
      }
    } catch (error: any) {
      const errorMessage =
        error?.response?.data?.detail || error?.message || 'Failed to save relation';

      if (error?.response?.status === 422 && error?.response?.data?.errors) {
        error.response.data.errors.forEach((err: any) => {
          if (err.field) {
            setError(err.field as any, {
              type: 'server',
              message: err.message,
            });
          }
        });
      }
    }
  };

  const handleCancel = () => {
    if (hasChanges) {
      setShowDiscardModal(true);
    } else {
      navigate('/entities/relations');
    }
  };

  const handleConfirmDiscard = () => {
    setShowDiscardModal(false);
    navigate('/entities/relations');
  };

  if (isEdit && isLoadingRelation) {
    return <div className="entity-form-loading">Loading...</div>;
  }

  return (
    <>
      <EntityFormTemplate
        title={isEdit ? 'Edit Relationship' : 'Create Relationship'}
        subtitle={isEdit ? 'Edit entity relationship' : 'Create a new relationship'}
        isEdit={isEdit}
        isLoading={isSubmitting}
        error={null}
        onSubmit={handleReactFormSubmit(onSubmit)}
        onCancel={handleCancel}
        submitButtonLabel={isEdit ? 'Save Changes' : 'Create Relationship'}
      >
        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Relationship Entities</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Source Entity"
              required
              error={errors.sourceEntity?.message}
              htmlFor="sourceEntity"
            >
              <input
                id="sourceEntity"
                type="text"
                placeholder="Enter source entity ID"
                {...register('sourceEntity')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Target Entity"
              required
              error={errors.targetEntity?.message}
              htmlFor="targetEntity"
            >
              <input
                id="targetEntity"
                type="text"
                placeholder="Enter target entity ID"
                {...register('targetEntity')}
          
              <FormFieldWrapper
                label="Confidence"
                error={errors.confidence?.message}
                htmlFor="confidence"
                helpText="0.0 to 1.0"
              >
                <input
                  id="confidence"
                  type="number"
                  min="0"
                  max="1"
                  step="0.01"
                  placeholder="0.8"
                  {...register('confidence', { valueAsNumber: true })}
                />
              </FormFieldWrapper>
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Relationship Type"
              required
              error={errors.type?.message}
              htmlFor="type"
            >
              <input
                id="type"
                type="text"
                placeholder="e.g., Owns, Integrates, Processes"
                {...register('type')}

          <div className="entity-form-section">
            <h3 className="entity-form-section-title">Effective Dates</h3>

            <div className="entity-form-section-grid">
              <FormFieldWrapper
                label="Effective From"
                error={errors.effectiveFrom?.message}
                htmlFor="effectiveFrom"
              >
                <input id="effectiveFrom" type="date" {...register('effectiveFrom')} />
              </FormFieldWrapper>

              <FormFieldWrapper
                label="Effective To"
                error={errors.effectiveTo?.message}
                htmlFor="effectiveTo"
              >
                <input id="effectiveTo" type="date" {...register('effectiveTo')} />
              </FormFieldWrapper>
            </div>
          </div>
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Direction"
              required
              error={errors.direction?.message}
              htmlFor="direction"
            >
              <select id="direction" {...register('direction')}>
                <option value="Unidirectional">Unidirectional</option>
                <option value="Bidirectional">Bidirectional</option>
              </select>
            </FormFieldWrapper>
          </div>

          <FormFieldWrapper
            label="Description"
            error={errors.description?.message}
            htmlFor="description"
          >
            <textarea
              id="description"
              placeholder="Describe the relationship..."
              {...register('description')}
            />
          </FormFieldWrapper>
        </div>

        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Relationship Properties</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Strength"
              error={errors.strength?.message}
              htmlFor="strength"
            >
              <select id="strength" {...register('strength')}>
                <option value="Required">Required</option>
                <option value="Optional">Optional</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Cardinality"
              error={errors.cardinality?.message}
              htmlFor="cardinality"
            >
              <select id="cardinality" {...register('cardinality')}>
                <option value="1:1">1:1 (One to One)</option>
                <option value="1:N">1:N (One to Many)</option>
                <option value="N:N">N:N (Many to Many)</option>
              </select>
            </FormFieldWrapper>
          </div>
        </div>
      </EntityFormTemplate>

      <DiscardChangesModal
        isOpen={showDiscardModal}
        onConfirm={handleConfirmDiscard}
        onCancel={() => setShowDiscardModal(false)}
      />
    </>
  );
}
